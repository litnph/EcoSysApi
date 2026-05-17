using FluentValidation;

namespace PFP.Application.Features.FeatureFlags.CreateFeatureFlagOverride;

public sealed class CreateFeatureFlagOverrideCommandValidator : AbstractValidator<CreateFeatureFlagOverrideCommand>
{
    public CreateFeatureFlagOverrideCommandValidator()
    {
        RuleFor(x => x.FlagId).NotEmpty();
        RuleFor(x => x.TargetId).NotEmpty();
        RuleFor(x => x.TargetType).IsInEnum();
    }
}
