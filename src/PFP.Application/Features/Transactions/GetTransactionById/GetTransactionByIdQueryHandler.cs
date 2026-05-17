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
            .Include(t => t.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Transaction was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(entity.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read this transaction.");

        if (_currentUser.CurrentOrgId is { } orgId && entity.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this transaction.");

        return new GetTransactionByIdResponse(MapDetail(entity));
    }

    private static TransactionDetailDto MapDetail(FinTransaction t)
    {
        TransactionSourceSummaryDto? src = t.Source is null
            ? null
            : new TransactionSourceSummaryDto(t.Source.Id, t.Source.Name, t.Source.Currency, t.Source.Balance);

        TransactionCategorySummaryDto? cat = t.Category is null
            ? null
            : new TransactionCategorySummaryDto(t.Category.Id, t.Category.Name, t.Category.Kind);

        return new TransactionDetailDto(
            t.Id,
            t.SmoduleId,
            t.Type,
            t.Status,
            t.Amount,
            t.Currency,
            t.TxnDate,
            t.SourceId,
            t.CategoryId,
            t.Description,
            t.Note,
            t.BillingCycleId,
            t.MonthlyPeriodId,
            t.RefTxnId,
            t.CreatedAt,
            t.UpdatedAt,
            t.Version,
            src,
            cat);
    }
}
