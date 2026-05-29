namespace PFP.Application.Features.Sources.CreateBalanceAdjustment;

/// <summary>API body for POST …/sources/{id}/balance-adjustments.</summary>
public sealed record CreateBalanceAdjustmentBody(
    long Amount,
    DateOnly TxnDate,
    string Note);
