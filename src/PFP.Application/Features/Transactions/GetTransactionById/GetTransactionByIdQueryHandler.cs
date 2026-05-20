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
return new GetTransactionByIdResponse(MapDetail(entity));
    }

    private static TransactionDetailDto MapDetail(FinTransaction t)
    {
        return TransactionDtoMapper.ToDetail(t);
    }
}
