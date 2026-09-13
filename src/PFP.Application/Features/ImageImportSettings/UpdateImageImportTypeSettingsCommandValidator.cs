using FluentValidation;

namespace PFP.Application.Features.ImageImportSettings;

public sealed class UpdateImageImportTypeSettingsCommandValidator
    : AbstractValidator<UpdateImageImportTypeSettingsCommand>
{
    public UpdateImageImportTypeSettingsCommandValidator()
    {
        RuleFor(x => x.Settings)
            .NotNull()
            .Must(settings => settings is not null
                && settings.Count == ImageImportTypeSettingsRules.SupportedTypes.Count)
            .WithMessage("All supported image import types must be configured.");
        RuleFor(x => x.Settings)
            .Must(settings => settings is null || settings
                .Select(setting => setting.Type?.Trim().ToLowerInvariant() ?? string.Empty)
                .Distinct(StringComparer.Ordinal)
                .Count() == settings.Count)
            .WithMessage("Image import type settings must not contain duplicate types.");
        RuleForEach(x => x.Settings).ChildRules(setting =>
        {
            setting.RuleFor(item => item.Type)
                .NotEmpty()
                .Must(type => !string.IsNullOrWhiteSpace(type)
                    && ImageImportTypeSettingsRules.SupportedTypes.Contains(type.Trim().ToLowerInvariant()))
                .WithMessage("Image import type is not supported.");
            setting.RuleFor(item => item.DisplayName)
                .NotEmpty()
                .MaximumLength(80);
        });
    }
}
