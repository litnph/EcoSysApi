using MediatR;
using PFP.Application.Features.BillingCycles.Commands.RefreshBillingCycle;

namespace PFP.Application.Features.BillingCycles.Commands.RemoveBillingCycleItem;

public sealed record RemoveBillingCycleItemCommand(Guid CycleId, Guid TransactionId)
    : IRequest<RefreshBillingCycleResponse>;
