using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.FileAttachments.Common;

/// <summary>Shared ACL checks tying attachment rows back to finance entities.</summary>
public static class FileAttachmentEntityAccess
{
    /// <summary>Ensures the caller can operate on attachments for this entity anchor.</summary>
    public static async Task<FinTransaction> RequireAttachmentTargetAsync(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        string entityType,
        Guid entityId,
        SpaceRole minimumRole,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!string.Equals(entityType, nameof(FinTransaction), StringComparison.Ordinal))
            throw new BusinessRuleException("Attachments are not supported for this entity type.");

        return await RequireFinTransactionAsync(db, currentUser, entityId, minimumRole, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Loads <see cref="FinTransaction"/> after module / org ACL checks.</summary>
    private static async Task<FinTransaction> RequireFinTransactionAsync(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        Guid transactionId,
        SpaceRole minimumRole,
        CancellationToken cancellationToken)
    {
        var entity = await db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Transaction was not found.");

        if (!await currentUser
                .HasSpaceModuleAccessAsync(entity.SmoduleId, minimumRole, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage files for this transaction.");

        if (currentUser.CurrentOrgId is { } orgId && entity.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this transaction.");

        return entity;
    }
}
