using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Spaces.MoveSpace;

/// <summary>FluentValidation rules for <see cref="MoveSpaceCommand"/>.</summary>
public sealed class MoveSpaceCommandValidator : AbstractValidator<MoveSpaceCommand>
{
    /// <summary>Registers validation rules.</summary>
    public MoveSpaceCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SpaceId).NotEmpty();

        RuleFor(x => x.NewParentId).Must(id => id is null || id != Guid.Empty);

        RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
        {
            var space = await db.Spaces.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == cmd.SpaceId, ct).ConfigureAwait(false);

            if (space is null)
            {
                ctx.AddFailure(nameof(cmd.SpaceId), "Space was not found.");
                return;
            }

            if (cmd.NewParentId is null)
            {
                await VerifySubtreeDepthAllowsMoveAsync(db, ctx, space, newDepthRoot: 0, ct).ConfigureAwait(false);
                return;
            }

            if (cmd.NewParentId == cmd.SpaceId)
            {
                ctx.AddFailure(nameof(cmd.NewParentId), "Cannot move a space under itself.");
                return;
            }

            var parent = await db.Spaces.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == cmd.NewParentId, ct).ConfigureAwait(false);

            if (parent is null)
            {
                ctx.AddFailure(nameof(cmd.NewParentId), "New parent space was not found.");
                return;
            }

            if (parent.OrgId != space.OrgId)
            {
                ctx.AddFailure(nameof(cmd.NewParentId), "New parent must belong to the same organisation.");
                return;
            }

            if (parent.Path == space.Path || parent.Path.StartsWith(space.Path + "/", StringComparison.Ordinal))
            {
                ctx.AddFailure(nameof(cmd.NewParentId), "Cannot move a space under one of its own descendants.");
                return;
            }

            await VerifySubtreeDepthAllowsMoveAsync(db, ctx, space, parent.Depth + 1, ct).ConfigureAwait(false);
        });
    }

    private static async Task VerifySubtreeDepthAllowsMoveAsync(
        IApplicationDbContext db,
        ValidationContext<MoveSpaceCommand> ctx,
        Space space,
        int newDepthRoot,
        CancellationToken ct)
    {
        var depthDelta = newDepthRoot - space.Depth;

        var subtreeMaxDepth = await db.Spaces.AsNoTracking()
            .Where(s => s.OrgId == space.OrgId &&
                        (s.Path == space.Path || s.Path.StartsWith(space.Path + "/")))
            .MaxAsync(s => (int?)s.Depth, ct).ConfigureAwait(false) ?? space.Depth;

        var newMaxDepth = subtreeMaxDepth + depthDelta;

        if (newMaxDepth > 5)
            ctx.AddFailure(nameof(MoveSpaceCommand.NewParentId), "The subtree would exceed depth 5.");
    }
}
