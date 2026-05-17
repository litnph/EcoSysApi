namespace PFP.Application.Features.InstallmentPlans.Commands.ProcessConversionFee;

/// <summary>Aggregated outcome of <see cref="ProcessConversionFeeCommand"/>.</summary>
public sealed record ProcessConversionFeeResponse(int PlansProcessed, int Errors);
