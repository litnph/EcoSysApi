using MediatR;
using PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;

namespace PFP.Application.Features.BillingCycles.Commands.DeleteBillingCycle;

public sealed record DeleteBillingCycleCommand(Guid CycleId)
    : IRequest<GenerateBillingCycleResponse>;
