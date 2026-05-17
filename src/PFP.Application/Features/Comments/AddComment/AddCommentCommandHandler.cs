using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.TagsComments.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Comments.AddComment;

public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, AddCommentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddCommentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AddCommentResponse> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var type = request.EntityType.Trim();
        if (!string.Equals(type, nameof(FinTransaction), StringComparison.Ordinal))
            throw new BusinessRuleException("Commenting on this entity kind is not supported yet.");

        var moduleNorm = string.IsNullOrWhiteSpace(request.ModuleCode)
            ? TagCommentConsts.FinanceModuleCode
            : request.ModuleCode.Trim().ToLowerInvariant();

        await FinanceModuleAccessHelper
            .RequireFinTransactionAnchorAsync(_db, _currentUser, request.EntityId, SpaceRole.Viewer, cancellationToken)
            .ConfigureAwait(false);

        if (request.ParentId is { } pid)
        {
            var parent = await _db.Comments.FirstOrDefaultAsync(c => c.Id == pid, cancellationToken).ConfigureAwait(false);

            if (parent is null)
                throw new NotFoundException("Parent comment was not found.");

            if (!string.Equals(parent.ModuleCode, moduleNorm, StringComparison.Ordinal)
                || !string.Equals(parent.EntityType, type, StringComparison.Ordinal)
                || parent.EntityId != request.EntityId)
                throw new BusinessRuleException("Replies must stay on the same entity thread.");
        }

        var entity = new Comment
        {
            ModuleCode = moduleNorm,
            EntityType = type,
            EntityId = request.EntityId,
            ParentId = request.ParentId,
            Content = request.Content.Trim(),
            AuthorId = _currentUser.UserId!.Value,
        };

        _db.Comments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AddCommentResponse(entity.Id);
    }
}
