using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Common;

/// <summary>Authenticated-user checks for finance handlers (no space/module scoping).</summary>
public static class FinanceAccessHelper
{
    public static void RequireAuthenticated(ICurrentUserService currentUser)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedAppException("Authentication is required.");
    }

    public static async Task<FinTransaction> RequireTransactionAsync(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        RequireAuthenticated(currentUser);

        var entity = await db.FinTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Transaction was not found.");

        return entity;
    }

    public static Task<FinTransaction> RequireFinTransactionAnchorAsync(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        Guid transactionId,
        CancellationToken cancellationToken) =>
        RequireTransactionAsync(db, currentUser, transactionId, cancellationToken);
}
