using FluentValidation;

namespace PFP.Application.Features.Users.UpdateProfile;

/// <summary>FluentValidation rules for <see cref="UpdateProfileCommand"/>.</summary>
public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    private static readonly HashSet<string> AllowedThemes = new(StringComparer.Ordinal) { "light", "dark", "system" };

    /// <summary>Registers field rules.</summary>
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DisplayName).MaximumLength(120).When(x => x.DisplayName is not null);
        RuleFor(x => x.PhoneNumber).MaximumLength(32).When(x => x.PhoneNumber is not null);
        RuleFor(x => x.LanguageCode).NotEmpty().MaximumLength(16);
        RuleFor(x => x.Timezone).NotEmpty().MaximumLength(64);
        RuleFor(x => x.DateFormat).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Theme)
            .Must(t => AllowedThemes.Contains(t))
            .WithMessage("Theme must be one of: light, dark, system.");
    }
}
