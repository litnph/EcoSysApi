using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Spaces.CreateSpace;

/// <summary>FluentValidation rules for <see cref="CreateSpaceCommand"/>.</summary>
public sealed class CreateSpaceCommandValidator : AbstractValidator<CreateSpaceCommand>
{
    /// <summary>Registers validation rules.</summary>
    public CreateSpaceCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.OrgId).NotEmpty();

        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);

        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Description).MaximumLength(1024).When(x => x.Description is not null);

        RuleFor(x => x.SortOrder).Must(v => !v.HasValue || v.Value >= 0).When(x => x.SortOrder is not null);

        RuleFor(x => x.ParentId).Must(id => id is null || id != Guid.Empty);

        RuleFor(x => x.OrgId).MustAsync(
                async (orgId, ct) =>
                    await db.Organizations.AsNoTracking().AnyAsync(o => o.Id == orgId, ct).ConfigureAwait(false))
            .WithMessage("Organisation was not found.");

        RuleFor(x => x).CustomAsync(
            async (cmd, ctx, ct) =>
            {
                if (cmd.ParentId is null) return;

                var parent = await db.Spaces
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == cmd.ParentId, ct)
                    .ConfigureAwait(false);

                if (parent is null)
                {
                    ctx.AddFailure(nameof(cmd.ParentId), "Parent space was not found.");
                    return;
                }

                if (parent.OrgId != cmd.OrgId)
                    ctx.AddFailure(nameof(cmd.ParentId), "Parent space must belong to the same organisation.");

                if (parent.Depth >= 5)
                    ctx.AddFailure(nameof(cmd.ParentId), "The space tree cannot exceed depth 5 (parent depth must be below 5).");
            });
    }
}
