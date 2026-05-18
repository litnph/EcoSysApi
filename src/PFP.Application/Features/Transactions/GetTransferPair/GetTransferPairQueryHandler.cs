using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.GetTransferPair;

/// <summary>Returns the outbound (negative amount) and inbound (positive amount) rows for a transfer pair.</summary>
public sealed class GetTransferPairQueryHandler : IRequestHandler<GetTransferPairQuery, GetTransferPairResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetTransferPairQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc cref="IRequestHandler{GetTransferPairQuery, GetTransferPairResponse}.Handle" />
    public async Task<GetTransferPairResponse> Handle(GetTransferPairQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .Include(t => t.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Transaction was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(entity.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read this transaction.");

        if (_currentUser.CurrentOrgId is { } orgId && entity.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this transaction.");

        if (entity.Type != TransactionType.Transfer)
            throw new BusinessRuleException("This transaction is not a transfer.");

        if (entity.RefTxnId is null)
            throw new BusinessRuleException("This transfer row is not linked to a counterpart.");

        var counterpart = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == entity.RefTxnId.Value, cancellationToken)
            .ConfigureAwait(false);

        if (counterpart is null || counterpart.Type != TransactionType.Transfer)
            throw new BusinessRuleException("The linked transfer counterpart was not found.");

        if (counterpart.RefTxnId != entity.Id)
            throw new BusinessRuleException("Transfer pair references are inconsistent.");

        FinTransaction outbound;
        FinTransaction inbound;
        if (entity.Amount < 0)
        {
            outbound = entity;
            inbound = counterpart;
        }
        else if (entity.Amount > 0)
        {
            outbound = counterpart;
            inbound = entity;
        }
        else
            throw new BusinessRuleException("Invalid transfer amounts.");

        if (outbound.Amount >= 0 || inbound.Amount <= 0)
            throw new BusinessRuleException("Could not determine outbound and inbound legs for this transfer.");

        return new GetTransferPairResponse(MapDetail(outbound), MapDetail(inbound));
    }

    private static TransactionDetailDto MapDetail(FinTransaction t) => TransactionDtoMapper.ToDetail(t);
}
