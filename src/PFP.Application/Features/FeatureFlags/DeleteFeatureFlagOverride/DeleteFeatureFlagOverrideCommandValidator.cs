using FluentValidation;

namespace PFP.Application.Features.FeatureFlags.DeleteFeatureFlagOverride;

public sealed class DeleteFeatureFlagOverrideCommandValidator : AbstractValidator<DeleteFeatureFlagOverrideCommand>
{
    public DeleteFeatureFlagOverrideCommandValidator()
    {
        RuleFor(x => x.FlagId).NotEmpty();
        RuleFor(x => x.OverrideId).NotEmpty();
    }
}
