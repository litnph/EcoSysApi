using PFP.Application.Features.BillingCycles.Common;

namespace PFP.Application.Features.BillingCycles.Commands.PayBillingCycle;

/// <summary>Result of <see cref="PayBillingCycleCommand"/>.</summary>
public sealed record PayBillingCycleResponse(FinBillingCycleDto Cycle, Guid PaymentTransactionId);
