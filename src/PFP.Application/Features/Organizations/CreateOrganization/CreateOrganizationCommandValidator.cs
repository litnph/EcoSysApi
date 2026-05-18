using System.Text.RegularExpressions;
using FluentValidation;

namespace PFP.Application.Features.Organizations.CreateOrganization;

/// <summary>FluentValidation rules for <see cref="CreateOrganizationCommand"/>.</summary>
public sealed class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    private static readonly Regex SlugPattern = new("^[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?$", RegexOptions.Compiled);

    /// <summary>Registers field rules.</summary>
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(64)
            .Must(s => SlugPattern.IsMatch(s))
            .WithMessage("Slug must be lowercase, alphanumeric and may contain dashes.");
        RuleFor(x => x.DefaultCurrency)
            .Length(3)
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultCurrency));
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
    }
}
