using MediatR;

namespace PFP.Application.Features.BillingCycles.Commands.CloseBillingCycle;

/// <summary>Closes an open billing cycle and reconciles total deferred spend.</summary>
public sealed record CloseBillingCycleCommand(Guid CycleId) : IRequest<CloseBillingCycleResponse>;
