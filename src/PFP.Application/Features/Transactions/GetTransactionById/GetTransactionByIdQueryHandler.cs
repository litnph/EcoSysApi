using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.GetTransactionById;

/// <summary>Reads a single transaction row with joined source and category.</summary>
public sealed class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, GetTransactionByIdResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetTransactionByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Returns the transaction when the caller can read its parent module.</summary>
    /// <inheritdoc cref="IRequestHandler{GetTransactionByIdQuery, GetTransactionByIdResponse}.Handle" />
    public async Task<GetTransactionByIdResponse> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Transaction was not found.");

        var canEditAmount = await TransactionAmountEditPolicy
            .CanEditAmountAsync(_db, entity.Id, cancellationToken)
            .ConfigureAwait(false);

        var canDelete = await TransactionDeletePolicy
            .CanDeleteAsync(_db, entity, cancellationToken)
            .ConfigureAwait(false);

        var hasInstallmentPlan = await _db.FinInstallmentPlans
            .AsNoTracking()
            .AnyAsync(p => p.OriginalTxnId == entity.Id, cancellationToken)
            .ConfigureAwait(false);

        var tags = await TransactionTagQueries
            .ForTransactionAsync(_db, entity.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GetTransactionByIdResponse(
            TransactionDtoMapper.ToDetail(
                entity,
                canEditAmount,
                canDelete,
                hasInstallmentPlan,
                entity.InstallmentPlanId is not null,
                tags));
    }
}
