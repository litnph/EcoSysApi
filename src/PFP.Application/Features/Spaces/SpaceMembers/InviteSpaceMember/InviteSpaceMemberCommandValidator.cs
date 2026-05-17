using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Spaces.SpaceMembers.InviteSpaceMember;

public sealed class InviteSpaceMemberCommandValidator : AbstractValidator<InviteSpaceMemberCommand>
{
    public InviteSpaceMemberCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SpaceId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();

        RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
        {
            var space = await db.Spaces.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == cmd.SpaceId, ct)
                .ConfigureAwait(false);

            if (space is null)
            {
                ctx.AddFailure(nameof(cmd.SpaceId), "Space was not found.");
                return;
            }

            var isOrgMember = await db.OrgMembers.AsNoTracking()
                .AnyAsync(
                    m => m.OrgId == space.OrgId
                         && m.UserId == cmd.UserId
                         && m.IsActive,
                    ct)
                .ConfigureAwait(false);

            if (!isOrgMember)
                ctx.AddFailure(nameof(cmd.UserId), "The invited user must be an active organisation member.");

            var alreadyMember = await db.SpaceMembers
                .AnyAsync(
                    m => m.SpaceId == cmd.SpaceId && m.UserId == cmd.UserId && m.LeftAt == null,
                    ct)
                .ConfigureAwait(false);

            if (alreadyMember)
                ctx.AddFailure(nameof(cmd.UserId), "This user already has membership in the space.");
        });
    }
}
