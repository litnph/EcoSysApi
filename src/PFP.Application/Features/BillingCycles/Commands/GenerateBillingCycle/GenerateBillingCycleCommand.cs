using MediatR;

namespace PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;

/// <summary>Creates an open billing cycle for a credit-card source (current month or a chosen statement month).</summary>
public sealed record GenerateBillingCycleCommand(
    Guid SourceId,
    int? StatementYear = null,
    int? StatementMonth = null) : IRequest<GenerateBillingCycleResponse>;
