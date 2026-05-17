using PFP.Application.Features.BillingCycles.Common;

namespace PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;

/// <summary>Result of <see cref="GenerateBillingCycleCommand"/>.</summary>
public sealed record GenerateBillingCycleResponse(FinBillingCycleDto Cycle);
