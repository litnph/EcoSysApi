using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Spaces.SpaceMembers.RemoveSpaceMember;

public sealed class RemoveSpaceMemberCommandValidator : AbstractValidator<RemoveSpaceMemberCommand>
{
    public RemoveSpaceMemberCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SpaceId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
        {
            var space = await db.Spaces.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SpaceId, ct)
                .ConfigureAwait(false);

            if (space is null)
            {
                ctx.AddFailure(nameof(cmd.SpaceId), "Space was not found.");
                return;
            }

            var org = await db.Organizations.AsNoTracking().FirstAsync(o => o.Id == space.OrgId, ct)
                .ConfigureAwait(false);

            if (org.OwnerId == cmd.UserId)
                ctx.AddFailure(nameof(cmd.UserId), "The organisation owner cannot be removed from spaces.");
        });
    }
}
