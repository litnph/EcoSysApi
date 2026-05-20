using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.GetInstallmentPlans;

/// <summary>Lists installment plans for a finance module.</summary>
public sealed record GetInstallmentPlansQuery(InstallmentStatus? Status) : IRequest<GetInstallmentPlansResponse>;
