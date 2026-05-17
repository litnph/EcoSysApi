using FluentValidation;

namespace PFP.Application.Features.BillingCycles.GetBillingCycleDetail;

/// <summary>FluentValidation rules for <see cref="GetBillingCycleDetailQuery"/>.</summary>
public sealed class GetBillingCycleDetailQueryValidator : AbstractValidator<GetBillingCycleDetailQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetBillingCycleDetailQueryValidator()
    {
        RuleFor(x => x.CycleId).NotEmpty();
    }
}
