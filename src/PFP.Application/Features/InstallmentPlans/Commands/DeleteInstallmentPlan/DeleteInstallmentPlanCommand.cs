using MediatR;

namespace PFP.Application.Features.InstallmentPlans.Commands.DeleteInstallmentPlan;

public sealed record DeleteInstallmentPlanCommand(Guid PlanId) : IRequest<Unit>;
