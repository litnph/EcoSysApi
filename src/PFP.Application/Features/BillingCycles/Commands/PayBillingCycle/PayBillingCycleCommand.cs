using MediatR;

namespace PFP.Application.Features.BillingCycles.Commands.PayBillingCycle;

/// <summary>Posts a payment from another source toward a closed or overdue billing cycle.</summary>
public sealed record PayBillingCycleCommand(Guid CycleId, Guid PaymentSourceId, long Amount) : IRequest<PayBillingCycleResponse>;
