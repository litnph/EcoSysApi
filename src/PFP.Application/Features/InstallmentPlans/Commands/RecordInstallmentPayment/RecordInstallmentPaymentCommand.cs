using MediatR;

namespace PFP.Application.Features.InstallmentPlans.Commands.RecordInstallmentPayment;

/// <summary>Records a direct payment toward one installment period.</summary>
public sealed record RecordInstallmentPaymentCommand(
    Guid PlanId,
    int InstallmentNumber,
    Guid PaymentSourceId) : IRequest<RecordInstallmentPaymentResponse>;
