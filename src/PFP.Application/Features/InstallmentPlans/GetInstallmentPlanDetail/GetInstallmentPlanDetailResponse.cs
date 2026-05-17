using PFP.Application.Features.InstallmentPlans.Common;

namespace PFP.Application.Features.InstallmentPlans.GetInstallmentPlanDetail;

/// <summary>Response for <see cref="GetInstallmentPlanDetailQuery"/>.</summary>
public sealed record GetInstallmentPlanDetailResponse(InstallmentPlanDetailDto Plan);
