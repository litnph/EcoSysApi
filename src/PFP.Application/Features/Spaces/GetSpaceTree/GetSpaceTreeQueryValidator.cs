using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Spaces.GetSpaceTree;

/// <summary>FluentValidation rules for <see cref="GetSpaceTreeQuery"/>.</summary>
public sealed class GetSpaceTreeQueryValidator : AbstractValidator<GetSpaceTreeQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetSpaceTreeQueryValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.OrgId).NotEmpty();

        RuleFor(x => x.OrgId).MustAsync(
                async (orgId, ct) =>
                    await db.Organizations.AsNoTracking().AnyAsync(o => o.Id == orgId, ct).ConfigureAwait(false))
            .WithMessage("Organisation was not found.");
    }
}
