using MediatR;

namespace PFP.Application.Features.InstallmentPlans.Commands.ProcessConversionFee;

/// <summary>Posts pending conversion-fee deferred transactions onto a newly opened billing cycle.</summary>
public sealed record ProcessConversionFeeCommand(Guid BillingCycleId) : IRequest<ProcessConversionFeeResponse>;
