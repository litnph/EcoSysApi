using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Tags.Common;
using PFP.Application.Features.TagsComments.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Tags.GetEntitiesByTag;

public sealed class GetEntitiesByTagQueryHandler : IRequestHandler<GetEntitiesByTagQuery, GetEntitiesByTagResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetEntitiesByTagQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetEntitiesByTagResponse> Handle(GetEntitiesByTagQuery request, CancellationToken cancellationToken)
    {
        var tag = await _db.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TagId, cancellationToken)
            .ConfigureAwait(false);

        if (tag is null)
            throw new NotFoundException("Tag was not found.");
        var raw = await _db.EntityTags.AsNoTracking()
            .Where(e => e.TagId == tag.Id)
            .OrderBy(e => e.EntityType)
            .ThenBy(e => e.EntityId)
            .Select(e => new TaggedEntityRefDto(e.ModuleCode, e.EntityType, e.EntityId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var rows = raw.Distinct().ToList();

        return new GetEntitiesByTagResponse(rows);
    }
}
