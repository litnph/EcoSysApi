using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Tags.DeleteTag;

public sealed class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteTagCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _db.Tags
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (tag is null)
            throw new NotFoundException("Tag was not found.");
        if (tag.UsageCount != 0)
            throw new BusinessRuleException("Only tags without active assignments can be deleted.");

        var anyActiveLink = await _db.EntityTags
            .AnyAsync(e => e.TagId == tag.Id, cancellationToken)
            .ConfigureAwait(false);

        if (anyActiveLink)
            throw new BusinessRuleException("Only tags without active assignments can be deleted.");

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
