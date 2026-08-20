using MediatR;

namespace PFP.Application.Features.InstallmentPlans.Commands.DeleteInstallmentPlan;

public sealed record DeleteInstallmentPlanCommand(Guid PlanId, int? ExpectedVersion = null) : IRequest<Unit>;
