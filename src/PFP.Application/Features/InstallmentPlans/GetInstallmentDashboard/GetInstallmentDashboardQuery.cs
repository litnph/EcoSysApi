using MediatR;

namespace PFP.Application.Features.InstallmentPlans.GetInstallmentDashboard;

public sealed record GetInstallmentDashboardQuery : IRequest<GetInstallmentDashboardResponse>;
