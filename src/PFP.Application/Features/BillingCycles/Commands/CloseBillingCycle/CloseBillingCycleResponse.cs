using PFP.Application.Features.BillingCycles.Common;

namespace PFP.Application.Features.BillingCycles.Commands.CloseBillingCycle;

/// <summary>Result of <see cref="CloseBillingCycleCommand"/>.</summary>
public sealed record CloseBillingCycleResponse(FinBillingCycleDto Cycle);
