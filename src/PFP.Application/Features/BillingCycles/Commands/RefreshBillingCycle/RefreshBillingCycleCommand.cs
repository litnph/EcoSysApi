using MediatR;

namespace PFP.Application.Features.BillingCycles.Commands.RefreshBillingCycle;

/// <summary>Maps eligible deferred transactions into an open billing cycle and recalculates totals.</summary>
public sealed record RefreshBillingCycleCommand(Guid CycleId) : IRequest<RefreshBillingCycleResponse>;
