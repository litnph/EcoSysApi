using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.FileAttachments.Common;

/// <summary>Shared checks tying attachment rows back to finance entities.</summary>
public static class FileAttachmentEntityAccess
{
    /// <summary>Ensures the caller can operate on attachments for this entity anchor.</summary>
    public static async Task<FinTransaction> RequireAttachmentTargetAsync(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(entityType, nameof(FinTransaction), StringComparison.Ordinal))
            throw new BusinessRuleException("Attachments are not supported for this entity type.");

        return await FinanceAccessHelper.RequireTransactionAsync(db, currentUser, entityId, cancellationToken)
            .ConfigureAwait(false);
    }
}
