using FluentValidation;

namespace PFP.Application.Features.BillingCycles.GetBillingCycles;

/// <summary>FluentValidation rules for <see cref="GetBillingCyclesQuery"/>.</summary>
public sealed class GetBillingCyclesQueryValidator : AbstractValidator<GetBillingCyclesQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetBillingCyclesQueryValidator()
    {
        RuleFor(x => x.SmoduleId).NotEmpty();
    }
}
