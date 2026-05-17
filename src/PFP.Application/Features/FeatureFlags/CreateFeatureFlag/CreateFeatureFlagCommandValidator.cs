using FluentValidation;

namespace PFP.Application.Features.FeatureFlags.CreateFeatureFlag;

public sealed class CreateFeatureFlagCommandValidator : AbstractValidator<CreateFeatureFlagCommand>
{
    public CreateFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.RolloutPercentage).InclusiveBetween(0, 100);
    }
}
