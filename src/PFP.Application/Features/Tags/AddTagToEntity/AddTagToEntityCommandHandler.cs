using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Tags.AddTagToEntity;

public sealed class AddTagToEntityCommandHandler : IRequestHandler<AddTagToEntityCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddTagToEntityCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(AddTagToEntityCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var type = request.EntityType.Trim();
        if (!string.Equals(type, nameof(FinTransaction), StringComparison.Ordinal))
            throw new BusinessRuleException("Tagging this entity kind is not supported yet.");

        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == request.TagId, cancellationToken).ConfigureAwait(false);

        if (tag is null)
            throw new NotFoundException("Tag was not found.");

        var txn = await FinanceAccessHelper
            .RequireFinTransactionAnchorAsync(_db, _currentUser, request.EntityId, cancellationToken)
            .ConfigureAwait(false);

        if (await _db.EntityTags.AnyAsync(
                e => e.TagId == tag.Id && e.EntityType == type && e.EntityId == txn.Id,
                cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException("This entity already carries the requested tag.");

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            tag = await _db.Tags.FirstAsync(t => t.Id == tag.Id, ct).ConfigureAwait(false);

            var link = new EntityTag
            {
                TagId = tag.Id,
                EntityType = type,
                EntityId = txn.Id,
                TaggedBy = _currentUser.UserId!.Value,
            };

            _db.EntityTags.Add(link);
            tag.UsageCount++;
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
