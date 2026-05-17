using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Rewrites every <see cref="EntityState.Deleted"/> entry that targets a
/// <see cref="SoftDeletableEntity"/> into a soft-delete update (spec §3.3).
/// <para>
/// <b>Same-transaction guarantee.</b> The rewrite happens in <c>SavingChanges</c>, mutating the
/// change tracker in place. Because the entry's <see cref="EntityState"/> moves from
/// <c>Deleted</c> to <c>Modified</c> <i>before</i> EF generates the SQL batch, the resulting
/// <c>UPDATE</c> statement is part of the same SaveChanges call — and therefore the same DB
/// transaction — as the rest of the user's work.
/// </para>
/// <para>
/// Runs <b>first</b> in the interceptor chain so that downstream interceptors (History, Audit)
/// see <c>IsDeleted = true</c> and emit a <c>Deleted</c> change-type / action accordingly.
/// </para>
/// <para>
/// Note: <see cref="Domain.Entities.FinTransaction"/> additionally needs a reversal row per spec §4.2.
/// That logic lives in the delete handler — the interceptor is intentionally generic.
/// </para>
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the interceptor.</summary>
    public SoftDeleteInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Rewrite(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Rewrite(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Rewrite(DbContext? context)
    {
        if (context is null) return;
        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<SoftDeletableEntity>())
        {
            if (entry.State != EntityState.Deleted) continue;
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = now;
            entry.Entity.DeletedBy = userId;
        }
    }
}
