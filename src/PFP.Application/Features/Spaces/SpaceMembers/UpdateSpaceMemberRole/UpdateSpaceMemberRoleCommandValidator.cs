using FluentValidation;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.SpaceMembers.UpdateSpaceMemberRole;

public sealed class UpdateSpaceMemberRoleCommandValidator : AbstractValidator<UpdateSpaceMemberRoleCommand>
{
    public UpdateSpaceMemberRoleCommandValidator()
    {
        RuleFor(x => x.SpaceId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewRole).IsInEnum();
    }
}
