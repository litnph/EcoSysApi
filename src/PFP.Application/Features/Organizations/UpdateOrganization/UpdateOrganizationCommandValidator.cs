using FluentValidation;

namespace PFP.Application.Features.Organizations.UpdateOrganization;

/// <summary>FluentValidation rules for <see cref="UpdateOrganizationCommand"/>.</summary>
public sealed class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    /// <summary>Registers field rules.</summary>
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DefaultCurrency)
            .Length(3)
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultCurrency));
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
    }
}
