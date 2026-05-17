using MediatR;

namespace PFP.Application.Features.InstallmentPlans.Commands.CreateInstallmentPlan;

/// <summary>Converts a deferred credit-card transaction into an installment plan.</summary>
public sealed record CreateInstallmentPlanCommand(
    Guid OriginalTxnId,
    int TotalMonths,
    decimal InterestRate,
    decimal? ConversionFeeRate) : IRequest<CreateInstallmentPlanResponse>;
