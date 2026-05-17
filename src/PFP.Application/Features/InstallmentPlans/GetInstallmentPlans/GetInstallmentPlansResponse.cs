using PFP.Application.Features.InstallmentPlans.Common;

namespace PFP.Application.Features.InstallmentPlans.GetInstallmentPlans;

/// <summary>Response for <see cref="GetInstallmentPlansQuery"/>.</summary>
public sealed record GetInstallmentPlansResponse(IReadOnlyList<InstallmentPlanListItemDto> Items);
