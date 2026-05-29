using MediatR;
using PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;

namespace PFP.Application.Features.BillingCycles.Commands.UpdateBillingCycleReconciliation;

public sealed record UpdateBillingCycleReconciliationCommand(
    Guid CycleId,
    string? ReconciliationNote,
    long? IssuerStatementAmount) : IRequest<GenerateBillingCycleResponse>;
