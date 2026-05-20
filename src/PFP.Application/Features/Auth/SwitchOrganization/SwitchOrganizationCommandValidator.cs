using FluentValidation;

namespace PFP.Application.Features.Auth.SwitchOrganization;

/// <summary>FluentValidation rules for <see cref="SwitchOrganizationCommand"/>.</summary>
public sealed class SwitchOrganizationCommandValidator : AbstractValidator<SwitchOrganizationCommand>
{
    /// <summary>Registers field rules.</summary>
    public SwitchOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
    }
}
