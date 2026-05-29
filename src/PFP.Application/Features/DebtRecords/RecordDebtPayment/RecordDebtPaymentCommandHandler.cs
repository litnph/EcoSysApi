using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.CreateTransaction;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.RecordDebtPayment;

/// <summary>
/// Resolves the appropriate transaction type from the debt direction and forwards to
/// <see cref="CreateTransactionCommand"/> so the unified balance / history / audit pipeline runs.
/// </summary>
public sealed class RecordDebtPaymentCommandHandler : IRequestHandler<RecordDebtPaymentCommand, RecordDebtPaymentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IMediator _mediator;

    /// <summary>Creates the handler.</summary>
    public RecordDebtPaymentCommandHandler(IApplicationDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    /// <inheritdoc cref="IRequestHandler{RecordDebtPaymentCommand, RecordDebtPaymentResponse}.Handle" />
    public async Task<RecordDebtPaymentResponse> Handle(RecordDebtPaymentCommand request, CancellationToken cancellationToken)
    {
        var debt = await _db.FinDebtRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DebtRecordId, cancellationToken)
            .ConfigureAwait(false);

        if (debt is null)
            throw new NotFoundException("Debt record was not found.");

        if (debt.Status != DebtStatus.Active)
            throw new BusinessRuleException("This debt record is no longer active.");

        if (CurrencyUnits.FromWhole(request.Amount) > debt.RemainingAmount)
            throw new BusinessRuleException("Payment amount exceeds the remaining debt balance.");

        var txnType = debt.Direction switch
        {
            DebtDirection.Borrowed => TransactionType.DebtRepay,
            DebtDirection.Lent => TransactionType.LoanCollect,
            _ => throw new BusinessRuleException("Unsupported debt direction."),
        };

        var inner = new CreateTransactionCommand(
            Type: txnType,
            Amount: request.Amount,
            SourceId: request.SourceId,
            CategoryId: null,
            TxnDate: request.TxnDate,
            Note: request.Note,
            Description: null,
            MonthlyPeriodId: null,
            ToSourceId: null,
            PersonName: debt.PersonName,
            PersonContact: debt.PersonContact,
            DebtRecordId: debt.Id,
            DueDate: null,
            Splits: null);

        var inner_result = await _mediator.Send(inner, cancellationToken).ConfigureAwait(false);
        return new RecordDebtPaymentResponse(inner_result.Transaction);
    }
}
