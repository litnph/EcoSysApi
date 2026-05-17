using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Spaces.SpaceMembers.GetSpaceMembers;

public sealed class GetSpaceMembersQueryValidator : AbstractValidator<GetSpaceMembersQuery>
{
    public GetSpaceMembersQueryValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SpaceId).NotEmpty();

        RuleFor(x => x.SpaceId).MustAsync(
                async (id, ct) => await db.Spaces.AsNoTracking().AnyAsync(s => s.Id == id, ct))
            .WithMessage("Space was not found.");
    }
}
