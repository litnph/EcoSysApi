namespace PFP.Application.Features.InstallmentPlans.Commands.RecordInstallmentPayment;

/// <summary>Result of <see cref="RecordInstallmentPaymentCommand"/>.</summary>
public sealed record RecordInstallmentPaymentResponse(Guid TransactionId);
