using MediatR;
using PFP.Application.Features.BillingCycles.Commands.RefreshBillingCycle;

namespace PFP.Application.Features.BillingCycles.Commands.AddBillingCycleItem;

public sealed record AddBillingCycleItemCommand(Guid CycleId, Guid TransactionId)
    : IRequest<RefreshBillingCycleResponse>;
