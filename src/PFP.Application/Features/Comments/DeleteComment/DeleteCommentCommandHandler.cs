using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.TagsComments.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Comments.DeleteComment;

public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteCommentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.Comments
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Comment was not found.");

        if (entity.AuthorId != _currentUser.UserId)
            throw new UnauthorizedAppException("Only the comment author may delete.");

        await EnsureAnchorAccessAsync(entity, cancellationToken).ConfigureAwait(false);

        var hasReplies = await _db.Comments.AsNoTracking().AnyAsync(
                c => c.ParentId == entity.Id,
                cancellationToken)
            .ConfigureAwait(false);

        if (hasReplies)
        {
            entity.Content = TagCommentConsts.DeletedCommentPlaceholder;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }

        _db.Comments.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }

    private async Task EnsureAnchorAccessAsync(Comment entity, CancellationToken cancellationToken)
    {
        var type = entity.EntityType;
        if (string.Equals(type, nameof(FinTransaction), StringComparison.Ordinal))
        {
            await FinanceModuleAccessHelper.RequireFinTransactionAnchorAsync(_db, _currentUser, entity.EntityId, SpaceRole.Viewer, cancellationToken).ConfigureAwait(false);
            return;
        }

        throw new BusinessRuleException("This comment anchor is not supported.");
    }
}
