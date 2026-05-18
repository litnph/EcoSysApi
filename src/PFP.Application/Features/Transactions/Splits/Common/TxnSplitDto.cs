using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Splits.Common;

/// <summary>One persisted split row (detail).</summary>
public sealed record TxnSplitDto(
    Guid Id,
    Guid TransactionId,
    string PersonName,
    string? PersonContact,
    long Amount,
    SplitStatus Status,
    DateTime? SettledAt,
    Guid? SettledTxnId);
