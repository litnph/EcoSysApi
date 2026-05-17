using FluentValidation;

namespace PFP.Application.Features.Translations.GetEntityTranslations;

/// <summary>Validates <see cref="GetEntityTranslationsQuery"/>.</summary>
public sealed class GetEntityTranslationsQueryValidator : AbstractValidator<GetEntityTranslationsQuery>
{
    /// <summary>Creates the validator.</summary>
    public GetEntityTranslationsQueryValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.EntityId).NotEmpty();
    }
}
