using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Tags.Common;
using PFP.Application.Features.TagsComments.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Tags.CreateTag;

public sealed class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, CreateTagResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateTagCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateTagResponse> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await _db.Tags.AnyAsync(
                t => t.Name == name,
                cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException("A tag with this name already exists in this module.");

        var entity = new Tag
        {            Name = name,
            Color = request.Color.Trim(),
            UsageCount = 0,
        };

        _db.Tags.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateTagResponse(entity.Id);
    }
}
