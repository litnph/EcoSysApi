using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Appends a row to the matching <c>*_HISTORY</c> table for every create / update / soft-delete of a
/// <see cref="VersionedEntity"/> that owns a history companion (spec §3.3, §3.7).
/// <para>
/// <b>Same-transaction guarantee.</b> Both the <see cref="VersionedEntity.Version"/> bump on the
/// parent entity and the new history row are applied to the change tracker in
/// <c>SavingChanges</c>, before EF Core flushes the SQL batch. They consequently ride the same
/// implicit (or explicit) DB transaction as the originating write: an exception during commit
/// rolls back both the parent update and its history row, preventing version skew.
/// </para>
/// <para>
/// Discovery of the entity → history pairing is convention-based: a history type is detected when
/// it derives from <see cref="VersionHistoryEntity"/> and its name is <c>{EntityName}History</c>
/// (e.g. <c>FinTransactionHistory</c> ↔ <c>FinTransaction</c>). Entities lacking a history
/// counterpart still have their <see cref="VersionedEntity.Version"/> incremented but no row is
/// appended.
/// </para>
/// <para>
/// <see cref="FinTransaction"/> history rows may be inserted manually by handlers (e.g. create flow);
/// when a matching <see cref="FinTransactionHistory"/> row is already tracked as <c>Added</c>, this
/// interceptor skips emitting a duplicate row for the same transaction id.
/// </para>
/// <para>
/// Runs <b>after</b> <see cref="SoftDeleteInterceptor"/> so that soft-delete writes are recorded
/// with <see cref="HistoryChangeType.Deleted"/>; runs <b>before</b> <see cref="AuditInterceptor"/>
/// so audit snapshots include the bumped <see cref="VersionedEntity.Version"/>.
/// </para>
/// </summary>
public sealed class HistoryInterceptor : SaveChangesInterceptor
{
    private static readonly IReadOnlyDictionary<Type, Type> HistoryMap = BuildHistoryMap();
    private static readonly ConcurrentDictionary<Type, PropertyInfo> HistoryEntityIdSetters = new();

    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the interceptor.</summary>
    public HistoryInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Process(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Process(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Process(DbContext? context)
    {
        if (context is null) return;

        // Snapshot the entries to a list so newly-added History rows (added below) don't re-enter the loop.
        var versionedEntries = context.ChangeTracker
            .Entries<VersionedEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        if (versionedEntries.Count == 0) return;

        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId;
        var sessionId = _currentUser.SessionId;

        foreach (var entry in versionedEntries)
        {
            if (entry.Entity is FinTransaction txnSkip
                && entry.State == EntityState.Modified
                && txnSkip.IsDeleted
                && ManualFinTransactionDeletedHistoryExists(context, txnSkip.Id))
            {
                continue;
            }

            HistoryChangeType changeType;
            if (entry.State == EntityState.Added)
            {
                changeType = HistoryChangeType.Created;
                entry.Entity.Version = 1;
            }
            else
            {
                entry.Entity.Version += 1;
                changeType = ResolveModifiedChangeType(entry);
            }

            entry.Entity.UpdatedBy = userId;
            entry.Entity.LastSessionId = sessionId;

            if (!HistoryMap.TryGetValue(entry.Metadata.ClrType, out var historyType)) continue;

            if (entry.Entity is FinTransaction txn && entry.State == EntityState.Added
                && ManualFinTransactionHistoryExists(context, txn.Id))
                continue;

            var history = (VersionHistoryEntity)Activator.CreateInstance(historyType)!;
            history.Version = entry.Entity.Version;
            history.ChangedBy = userId;
            history.SessionId = sessionId;
            history.ChangeType = changeType;
            history.ChangedFields = changeType is HistoryChangeType.Updated or HistoryChangeType.Deleted
                ? EntitySnapshot.ChangedFields(entry)
                : null;
            history.Snapshot = EntitySnapshot.Snapshot(entry, useOriginal: false);
            history.CreatedAt = now;
            history.UpdatedAt = now;

            if (changeType == HistoryChangeType.Cancelled && entry.Entity is FinInstallmentPlan cancelledPlan)
                history.ChangeReason = cancelledPlan.CancellationReason;

            var idSetter = HistoryEntityIdSetters.GetOrAdd(historyType, ResolveHistoryEntityIdSetter);
            idSetter.SetValue(history, entry.Entity.Id);

            context.Add(history);
        }
    }

    private static bool ManualFinTransactionHistoryExists(DbContext context, Guid transactionId) =>
        context.ChangeTracker.Entries<FinTransactionHistory>()
            .Any(e => e.State == EntityState.Added && e.Entity.TransactionId == transactionId);

    private static bool ManualFinTransactionDeletedHistoryExists(DbContext context, Guid transactionId) =>
        context.ChangeTracker.Entries<FinTransactionHistory>()
            .Any(e => e.State == EntityState.Added
                      && e.Entity.TransactionId == transactionId
                      && e.Entity.ChangeType == HistoryChangeType.Deleted);

    private static bool IsRestore(EntityEntry<VersionedEntity> entry)
    {
        var isDeletedProp = entry.Property(nameof(SoftDeletableEntity.IsDeleted));
        return isDeletedProp.IsModified
               && (bool)(isDeletedProp.OriginalValue ?? false)
               && !(bool)(isDeletedProp.CurrentValue ?? false);
    }

    private static HistoryChangeType ResolveModifiedChangeType(EntityEntry<VersionedEntity> entry)
    {
        if (entry.Entity is FinInstallmentPlan plan
            && entry.Property(nameof(FinInstallmentPlan.Status)).IsModified
            && plan.Status == InstallmentStatus.Cancelled)
            return HistoryChangeType.Cancelled;

        if (IsRestore(entry))
            return HistoryChangeType.Restored;

        if (entry.Entity.IsDeleted)
            return HistoryChangeType.Deleted;

        return HistoryChangeType.Updated;
    }

    private static Dictionary<Type, Type> BuildHistoryMap()
    {
        var dict = new Dictionary<Type, Type>();
        var assembly = typeof(VersionHistoryEntity).Assembly;
        foreach (var historyType in assembly.GetTypes())
        {
            if (historyType.IsAbstract) continue;
            if (!typeof(VersionHistoryEntity).IsAssignableFrom(historyType)) continue;
            if (!historyType.Name.EndsWith("History", StringComparison.Ordinal)) continue;

            var entityName = historyType.Name[..^"History".Length];
            var entityType = assembly.GetTypes().FirstOrDefault(t =>
                t.Name == entityName
                && typeof(VersionedEntity).IsAssignableFrom(t)
                && t.Namespace is "PFP.Domain.Entities" or "PFP.Domain.Entities.Finance");
            if (entityType is null) continue;
            dict[entityType] = historyType;
        }
        return dict;
    }

    private static PropertyInfo ResolveHistoryEntityIdSetter(Type historyType) =>
        historyType.GetProperty("EntityId")
        ?? historyType.GetProperty(nameof(FinTransactionHistory.TransactionId))
        ?? throw new InvalidOperationException(
            $"History type {historyType.Name} must expose EntityId or TransactionId for FK wiring.");
}
