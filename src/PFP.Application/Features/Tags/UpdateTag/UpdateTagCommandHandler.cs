using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Tags.Common;
using PFP.Application.Features.TagsComments.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Tags.UpdateTag;

public sealed class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, UpdateTagResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateTagCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<UpdateTagResponse> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _db.Tags
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (tag is null)
            throw new NotFoundException("Tag was not found.");
        var name = request.Name.Trim();
        if (await _db.Tags.AnyAsync(
                t =>
                    t.Id == tag.Id
                    && t.Name == name
                    && t.Id != tag.Id,
                cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException("Another tag already uses this name.");

        tag.Name = name;
        tag.Color = request.Color.Trim();
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new UpdateTagResponse(tag.Id);
    }
}
