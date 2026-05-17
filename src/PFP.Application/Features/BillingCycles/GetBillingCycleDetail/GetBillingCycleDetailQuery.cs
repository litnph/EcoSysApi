using MediatR;

namespace PFP.Application.Features.BillingCycles.GetBillingCycleDetail;

/// <summary>Returns one billing cycle plus its linked transactions.</summary>
public sealed record GetBillingCycleDetailQuery(Guid CycleId) : IRequest<GetBillingCycleDetailResponse>;
