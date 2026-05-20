using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Members.DeleteMember;

public sealed class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteMemberCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == request.MemberId)
            throw new ForbiddenException("You cannot delete your own account.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.MemberId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
            throw new NotFoundException("Member was not found.");

        user.IsActive = false;
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
