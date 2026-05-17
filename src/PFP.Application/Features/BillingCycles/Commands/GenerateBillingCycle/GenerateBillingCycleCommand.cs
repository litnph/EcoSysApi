using MediatR;

namespace PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;

/// <summary>Creates the next open billing cycle for a credit-card source (manual / on-demand generation).</summary>
public sealed record GenerateBillingCycleCommand(Guid SourceId) : IRequest<GenerateBillingCycleResponse>;
