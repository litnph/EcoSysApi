using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Splits.GetPendingSplits;

/// <summary>Pending splits grouped under each parent expense transaction.</summary>
public sealed record GetPendingSplitsResponse(IReadOnlyList<PendingSplitGroupDto> Groups);

/// <summary>One parent transaction and its open split lines.</summary>
public sealed record PendingSplitGroupDto(
    Guid TransactionId,
    DateOnly TxnDate,
    long TransactionAmount,
    string Currency,
    string Description,
    IReadOnlyList<TxnSplitListDto> Splits);

/// <summary>Split row in a pending listing.</summary>
public sealed record TxnSplitListDto(
    Guid Id,
    string PersonName,
    string? PersonContact,
    long Amount,
    SplitStatus Status,
    DateTime CreatedAt);
