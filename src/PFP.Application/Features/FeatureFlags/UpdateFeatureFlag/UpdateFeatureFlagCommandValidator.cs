using FluentValidation;

namespace PFP.Application.Features.FeatureFlags.UpdateFeatureFlag;

public sealed class UpdateFeatureFlagCommandValidator : AbstractValidator<UpdateFeatureFlagCommand>
{
    public UpdateFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.RolloutPercentage).InclusiveBetween(0, 100);
    }
}
