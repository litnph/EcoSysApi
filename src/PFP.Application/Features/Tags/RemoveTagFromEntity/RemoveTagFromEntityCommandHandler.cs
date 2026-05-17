using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.TagsComments.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Tags.RemoveTagFromEntity;

public sealed class RemoveTagFromEntityCommandHandler : IRequestHandler<RemoveTagFromEntityCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RemoveTagFromEntityCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RemoveTagFromEntityCommand request, CancellationToken cancellationToken)
    {
        var type = request.EntityType.Trim();
        if (!string.Equals(type, nameof(FinTransaction), StringComparison.Ordinal))
            throw new BusinessRuleException("Untagging this entity kind is not supported yet.");

        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == request.TagId, cancellationToken).ConfigureAwait(false);

        if (tag is null)
            throw new NotFoundException("Tag was not found.");

        await FinanceModuleAccessHelper.RequireFinanceSmoduleAsync(_db, _currentUser, tag.SmoduleId, SpaceRole.Editor, cancellationToken).ConfigureAwait(false);

        _ = await FinanceModuleAccessHelper
            .RequireFinTransactionAnchorAsync(_db, _currentUser, request.EntityId, SpaceRole.Editor, cancellationToken)
            .ConfigureAwait(false);

        var link = await _db.EntityTags.FirstOrDefaultAsync(
                e => e.TagId == tag.Id && e.EntityType == type && e.EntityId == request.EntityId,
                cancellationToken)
            .ConfigureAwait(false);

        if (link is null)
            throw new NotFoundException("The tag assignment was not found.");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _db.EntityTags.Remove(link);
            tag = await _db.Tags.FirstAsync(t => t.Id == tag.Id, cancellationToken).ConfigureAwait(false);
            tag.UsageCount = Math.Max(0, tag.UsageCount - 1);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
