using PFP.Application.Features.BillingCycles.Common;

namespace PFP.Application.Features.BillingCycles.GetBillingCycleDetail;

/// <summary>Envelope returned by <see cref="GetBillingCycleDetailQuery"/>.</summary>
public sealed record GetBillingCycleDetailResponse(FinBillingCycleDetailDto Detail);
