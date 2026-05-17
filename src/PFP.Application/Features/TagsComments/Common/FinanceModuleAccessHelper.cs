using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.TagsComments.Common;

/// <summary>Centralised ACL for finance-<see cref="SpaceModule"/> and <see cref="FinTransaction"/> anchors.</summary>
public static class FinanceModuleAccessHelper
{
    /// <summary>
    /// Returns the enabled finance module when the caller satisfies <paramref name="minimumRole"/>
    /// (and JWT org aligns with owning space).
    /// </summary>
    public static async Task<SpaceModule> RequireFinanceSmoduleAsync(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        Guid smoduleId,
        SpaceRole minimumRole,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var smodule = await db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == smoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null || smodule.ModuleCode != ModuleCode.Finance || !smodule.IsEnabled)
            throw new NotFoundException("Finance module instance was not found.");

        if (!await currentUser
                .HasSpaceModuleAccessAsync(smodule.Id, minimumRole, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission for this finance module.");

        if (currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this finance module.");

        return smodule;
    }

    /// <summary>Ensures the caller may read/write the anchored <see cref="FinTransaction"/>.</summary>
    public static async Task<FinTransaction> RequireFinTransactionAnchorAsync(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        Guid transactionId,
        SpaceRole minimumRole,
        CancellationToken cancellationToken)
    {
        var txn = await db.FinTransactions
            .Include(t => t.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken)
            .ConfigureAwait(false);

        if (txn is null)
            throw new NotFoundException("Transaction was not found.");

        if (!await currentUser
                .HasSpaceModuleAccessAsync(txn.SmoduleId, minimumRole, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to access this transaction.");

        if (currentUser.CurrentOrgId is { } orgId && txn.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this transaction.");

        if (txn.Smodule.ModuleCode != ModuleCode.Finance || !txn.Smodule.IsEnabled)
            throw new BusinessRuleException("This transaction cannot be anchored for finance tagging.");

        return txn;
    }
}
