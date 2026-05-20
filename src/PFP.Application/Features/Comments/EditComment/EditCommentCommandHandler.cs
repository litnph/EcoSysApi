using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.TagsComments.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Comments.EditComment;

public sealed class EditCommentCommandHandler : IRequestHandler<EditCommentCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public EditCommentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(EditCommentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.Comments
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Comment was not found.");

        if (entity.AuthorId != _currentUser.UserId)
            throw new UnauthorizedAppException("Only the comment author may edit.");

        await EnsureAnchorAccessAsync(entity, cancellationToken).ConfigureAwait(false);

        entity.Content = request.Content.Trim();
        entity.IsEdited = true;
        entity.EditedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }

    private async Task EnsureAnchorAccessAsync(Comment entity, CancellationToken cancellationToken)
    {
        var type = entity.EntityType;
        if (string.Equals(type, nameof(FinTransaction), StringComparison.Ordinal))
        {
            await FinanceAccessHelper.RequireFinTransactionAnchorAsync(_db, _currentUser, entity.EntityId, cancellationToken).ConfigureAwait(false);
            return;
        }

        throw new BusinessRuleException("This comment anchor is not editable for the current MVP.");
    }
}
