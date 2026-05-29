namespace PFP.Application.Features.Sources.Common;

/// <summary>One row in a source balance ledger (opening line or transaction leg).</summary>
public sealed record SourceBalanceLedgerEntryDto(
    string EntryKind,
    Guid? TransactionId,
    string? TransactionType,
    DateOnly TxnDate,
    string Description,
    long Delta,
    long BalanceAfter);
