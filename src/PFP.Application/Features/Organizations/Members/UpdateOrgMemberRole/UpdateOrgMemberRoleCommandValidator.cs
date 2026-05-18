using FluentValidation;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.Members.UpdateOrgMemberRole;

/// <summary>FluentValidation rules for <see cref="UpdateOrgMemberRoleCommand"/>.</summary>
public sealed class UpdateOrgMemberRoleCommandValidator : AbstractValidator<UpdateOrgMemberRoleCommand>
{
    /// <summary>Registers field rules.</summary>
    public UpdateOrgMemberRoleCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.Role)
            .Must(r => r == OrgRole.Member || r == OrgRole.Admin)
            .WithMessage("Owner role transfer must use the dedicated transfer endpoint.");
    }
}
