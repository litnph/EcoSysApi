using PFP.Application.Features.BillingCycles.Common;

namespace PFP.Application.Features.BillingCycles.GetBillingCycles;

/// <summary>Envelope returned by <see cref="GetBillingCyclesQuery"/>.</summary>
public sealed record GetBillingCyclesResponse(IReadOnlyList<FinBillingCycleDto> Items);
