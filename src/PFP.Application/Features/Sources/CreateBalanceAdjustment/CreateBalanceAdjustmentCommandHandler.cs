using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Utils;
using PFP.Application.Features.Sources.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.CreateBalanceAdjustment;

public sealed class CreateBalanceAdjustmentCommandHandler
    : IRequestHandler<CreateBalanceAdjustmentCommand, CreateBalanceAdjustmentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateBalanceAdjustmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateBalanceAdjustmentResponse> Handle(
        CreateBalanceAdjustmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        return await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var source = await _db.FinSources
                .FirstOrDefaultAsync(s => s.Id == request.SourceId, ct)
                .ConfigureAwait(false);

            if (source is null || source.IsDeleted)
                throw new NotFoundException("Source was not found.");

            if (!AssetSourceRules.SupportsBalanceLedger(source.Type))
                throw new BusinessRuleException("Balance adjustments are not allowed on credit card sources.");

            var amount = CurrencyUnits.FromWhole(request.Amount);
            var note = request.Note.Trim();
            var description = "Điều chỉnh số dư";
            if (note.Length > 0)
            {
                var suffix = $" — {note}";
                description = (description + suffix).Length <= 512
                    ? description + suffix
                    : (description + suffix)[..512];
            }

            var txn = new FinTransaction
            {
                Type = TransactionType.BalanceAdjustment,
                Status = TxnStatus.New,
                Amount = amount,
                Currency = source.Currency,
                TxnDate = request.TxnDate,
                SourceId = source.Id,
                Description = description,
                Note = note.Length <= 500 ? note : note[..500],
            };

            _db.FinTransactions.Add(txn);
            source.Balance += amount;

            FinTransactionHistoryHelper.AddCreated(_db, _currentUser, txn);

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            return new CreateBalanceAdjustmentResponse(
                txn.Id,
                CurrencyUnits.ToWhole(source.Balance));
        }, cancellationToken).ConfigureAwait(false);
    }
}
