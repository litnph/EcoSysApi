using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Comments.Common;
using PFP.Application.Features.TagsComments.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Comments.GetComments;

public sealed class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, GetCommentsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCommentsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetCommentsResponse> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var type = request.EntityType.Trim();
        if (!string.Equals(type, nameof(FinTransaction), StringComparison.Ordinal))
            throw new BusinessRuleException("Listing comments for this entity kind is not supported yet.");

        var moduleNorm = string.IsNullOrWhiteSpace(request.ModuleCode)
            ? TagCommentConsts.FinanceModuleCode
            : request.ModuleCode.Trim().ToLowerInvariant();

        await FinanceAccessHelper
            .RequireFinTransactionAnchorAsync(_db, _currentUser, request.EntityId, cancellationToken)
            .ConfigureAwait(false);

        var rows = await (
                from c in _db.Comments.AsNoTracking()
                join u in _db.Users.AsNoTracking() on c.AuthorId equals u.Id
                where c.ModuleCode == moduleNorm
                      && c.EntityType == type
                      && c.EntityId == request.EntityId
                orderby c.CreatedAt ascending
                select new FlatDto(
                    c.Id,
                    c.ParentId,
                    c.Content,
                    c.AuthorId,
                    u.FullName,
                    c.CreatedAt,
                    c.IsEdited,
                    c.EditedAt))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

        var childLookup = rows
            .Where(r => r.ParentId is not null)
            .ToLookup(r => r.ParentId!.Value);

        CommentTreeNodeDto Build(FlatDto dto)
        {
            var replies = childLookup[dto.Id]
                .OrderBy(r => r.CreatedAtUtc)
                .Select(Build)
                .ToList();

            return new CommentTreeNodeDto(
                dto.Id,
                dto.ParentId,
                dto.Content,
                dto.AuthorId,
                dto.AuthorFullName,
                dto.CreatedAtUtc,
                dto.IsEdited,
                dto.EditedAtUtc,
                replies);
        }

        var threads = rows
            .Where(r => r.ParentId is null)
            .OrderBy(r => r.CreatedAtUtc)
            .Select(Build)
            .ToList();

        return new GetCommentsResponse(threads);
    }

    private sealed record FlatDto(
        Guid Id,
        Guid? ParentId,
        string Content,
        Guid AuthorId,
        string AuthorFullName,
        DateTime CreatedAtUtc,
        bool IsEdited,
        DateTime? EditedAtUtc);
}
