using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Members.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Members.UpdateMember;

public sealed class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, UpdateMemberResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateMemberCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IPasswordHasher passwordHasher)
    {
        _db = db;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    public async Task<UpdateMemberResponse> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.MemberId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
            throw new NotFoundException("Member was not found.");

        if (_currentUser.UserId == user.Id && !request.IsActive)
            throw new ForbiddenException("You cannot deactivate your own account.");

        if (_currentUser.UserId == user.Id && request.Role != UserRole.Admin)
            throw new ForbiddenException("You cannot remove your own admin role.");

        user.FullName = request.FullName.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new UpdateMemberResponse(new MemberDetailDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.IsActive,
            user.LastLoginAt,
            user.CreatedAt));
    }
}
