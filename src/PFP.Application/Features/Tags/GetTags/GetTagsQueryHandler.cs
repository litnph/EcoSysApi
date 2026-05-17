using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Tags.Common;
using PFP.Application.Features.TagsComments.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Tags.GetTags;

public sealed class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, IReadOnlyList<TagListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetTagsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TagListItemDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        await FinanceModuleAccessHelper.RequireFinanceSmoduleAsync(_db, _currentUser, request.SmoduleId, SpaceRole.Viewer, cancellationToken).ConfigureAwait(false);

        return await _db.Tags.AsNoTracking()
            .Where(t => t.SmoduleId == request.SmoduleId)
            .OrderBy(t => t.Name)
            .Select(t => new TagListItemDto(t.Id, t.SmoduleId, t.Name, t.Color, t.UsageCount))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
