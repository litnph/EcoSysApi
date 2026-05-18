using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.CreateTransaction;

/// <summary>
/// Creates a finance transaction (direct, income, transfer, deferred, or debt / loan flows).
/// </summary>
public sealed record CreateTransactionCommand(
    Guid SmoduleId,
    TransactionType Type,
    long Amount,
    Guid SourceId,
    Guid? CategoryId,
    DateOnly TxnDate,
    string? Note,
    Guid? MonthlyPeriodId,
    Guid? ToSourceId,
    Guid? BillingCycleId,
    string? PersonName,
    string? PersonContact,
    Guid? DebtRecordId,
    DateOnly? DueDate,
    IReadOnlyList<SplitItemDto>? Splits) : IRequest<CreateTransactionResponse>;
