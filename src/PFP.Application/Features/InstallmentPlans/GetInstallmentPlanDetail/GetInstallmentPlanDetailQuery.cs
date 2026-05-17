using MediatR;

namespace PFP.Application.Features.InstallmentPlans.GetInstallmentPlanDetail;

/// <summary>Returns one installment plan with its pay schedule.</summary>
public sealed record GetInstallmentPlanDetailQuery(Guid PlanId) : IRequest<GetInstallmentPlanDetailResponse>;
