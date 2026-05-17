using MediatR;

namespace PFP.Application.Features.InstallmentPlans.Commands.CancelInstallmentPlan;

/// <summary>Cancels an active installment plan.</summary>
public sealed record CancelInstallmentPlanCommand(Guid PlanId, string? Reason) : IRequest<Unit>;
