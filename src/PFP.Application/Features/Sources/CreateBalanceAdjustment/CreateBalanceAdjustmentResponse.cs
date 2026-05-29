namespace PFP.Application.Features.Sources.CreateBalanceAdjustment;

public sealed record CreateBalanceAdjustmentResponse(
    Guid TransactionId,
    long NewBalance);
