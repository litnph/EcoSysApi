using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// (1) Stamps <see cref="BaseEntity.CreatedAt"/> / <see cref="BaseEntity.UpdatedAt"/> on every
/// inserted / updated row, and (2) appends an <see cref="AuditLog"/> entry per write so that the
/// audit row is persisted in the same DB transaction as the originating change (spec §3.3).
/// <para>
/// <b>Same-transaction guarantee.</b> The interceptor hooks the <c>SavingChanges</c> phase and
/// pushes new <see cref="AuditLog"/> rows into the change tracker <i>before</i> EF generates the
/// SQL batch. EF Core then issues the user's writes and our audit rows together inside the
/// implicit transaction it opens around <c>SaveChanges</c> — or inside an explicit transaction
/// when the handler has wrapped <c>SaveChanges</c> in <see cref="Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction"/>.
/// Either way the audit row is committed atomically with the change it describes; a rollback
/// rolls both back.
/// </para>
/// <para>
/// Runs <b>last</b> in the interceptor chain so that:
/// <list type="bullet">
/// <item><see cref="SoftDeleteInterceptor"/> has already rewritten <c>Deleted</c> to <c>Modified</c>
/// — letting us detect a soft-delete via <c>IsDeleted</c>.</item>
/// <item><see cref="HistoryInterceptor"/> has already bumped <see cref="VersionedEntity.Version"/>
/// — so the <c>AfterSnapshot</c> reflects the final stored version.</item>
/// </list>
/// </para>
/// <para>
/// Self-audit is avoided by skipping entries whose entity type is <see cref="AuditLog"/>,
/// <see cref="SystemEventLog"/>, or any <see cref="VersionHistoryEntity"/>. Those rows also opt
/// out of <see cref="BaseEntity.CreatedAt"/> / <see cref="BaseEntity.UpdatedAt"/> stamping because
/// their producers (this interceptor, <see cref="HistoryInterceptor"/>) set the timestamps
/// themselves.
/// </para>
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the interceptor.</summary>
    public AuditInterceptor(ICurrentUserService currentUser)
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

        var now = DateTime.UtcNow;

        // Step 1 — stamp timestamps on every tracked BaseEntity that's about to be written.
        // Self-managed types (AuditLog, SystemEventLog, *_HISTORY) are skipped here because their
        // producers stamp CreatedAt / UpdatedAt explicitly at construction time.
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is AuditLog or SystemEventLog or VersionHistoryEntity) continue;

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
            }
        }

        // Step 2 — build audit rows for everything except self-audit / history tables.
        var auditableEntries = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity is not AuditLog
                        and not SystemEventLog
                        and not VersionHistoryEntity)
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (auditableEntries.Count == 0) return;

        var pendingManualSoftDeleteAudits = context.ChangeTracker.Entries<AuditLog>()
            .Where(e => e.State == EntityState.Added && e.Entity.Action == AuditAction.Deleted)
            .Select(e => e.Entity.EntityId)
            .ToHashSet();

        var userId = _currentUser.UserId;
        var sessionId = _currentUser.SessionId;
        var ipAddress = _currentUser.IpAddress;
        var userAgent = _currentUser.UserAgent;

        foreach (var entry in auditableEntries)
        {
            if (entry.Entity is FinTransaction && pendingManualSoftDeleteAudits.Contains(entry.Entity.Id))
                continue;

            var action = ResolveAction(entry);
            var auditLog = new AuditLog
            {
                UserId = userId,
                SessionId = sessionId,
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = entry.Entity.Id,
                Action = action,
                BeforeSnapshot = action == AuditAction.Created
                    ? null
                    : EntitySnapshot.Snapshot(entry, useOriginal: true),
                AfterSnapshot = entry.State == EntityState.Deleted
                    ? null
                    : EntitySnapshot.Snapshot(entry, useOriginal: false),
                ChangedFields = action == AuditAction.Updated
                    ? EntitySnapshot.ChangedFields(entry)
                    : null,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = now,
                UpdatedAt = now,
            };
            context.Add(auditLog);
        }
    }

    private static AuditAction ResolveAction(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        // Hard-delete (no SoftDeletableEntity rewrite happened) — should be rare in practice.
        if (entry.State == EntityState.Deleted) return AuditAction.Deleted;
        if (entry.State == EntityState.Added) return AuditAction.Created;

        // Modified state: distinguish a soft-delete (IsDeleted flipped false → true) from a regular update.
        if (entry.Entity is SoftDeletableEntity)
        {
            var isDeletedProp = entry.Property(nameof(SoftDeletableEntity.IsDeleted));
            var wasDeleted = (bool)(isDeletedProp.OriginalValue ?? false);
            var nowDeleted = (bool)(isDeletedProp.CurrentValue ?? false);
            if (!wasDeleted && nowDeleted) return AuditAction.Deleted;
        }

        return AuditAction.Updated;
    }
}
