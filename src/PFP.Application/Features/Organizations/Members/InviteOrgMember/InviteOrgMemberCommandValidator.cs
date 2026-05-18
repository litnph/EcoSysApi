using FluentValidation;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.Members.InviteOrgMember;

/// <summary>FluentValidation rules for <see cref="InviteOrgMemberCommand"/>.</summary>
public sealed class InviteOrgMemberCommandValidator : AbstractValidator<InviteOrgMemberCommand>
{
    /// <summary>Registers field rules.</summary>
    public InviteOrgMemberCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role)
            .Must(r => r == OrgRole.Member || r == OrgRole.Admin)
            .WithMessage("Invitations can only grant Member or Admin role.");
    }
}
