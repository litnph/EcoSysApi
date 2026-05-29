using PFP.Application.Features.BillingCycles.Common;

namespace PFP.Application.Features.BillingCycles.Commands.RefreshBillingCycle;

public sealed record RefreshBillingCycleResponse(
    FinBillingCycleDto Cycle,
    int AddedCount,
    int SkippedCount);
