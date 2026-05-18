using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Users.Common;

namespace PFP.Application.Features.Users.GetProfile;

/// <summary>Loads <c>USERS</c>, <c>USER_PROFILES</c>, and the active avatar in a single query.</summary>
public sealed class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, GetProfileResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetProfileQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Email, u.FullName, u.IsEmailVerified })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
            throw new NotFoundException("User was not found.");

        var profile = await _db.UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        var avatarUrl = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.AvatarUploads)
            .Where(a => a.IsActive)
            .Select(a => a.StorageUrl)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var dto = new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.IsEmailVerified,
            profile?.LanguageCode ?? "vi",
            profile?.Timezone ?? "Asia/Ho_Chi_Minh",
            profile?.DateFormat ?? "dd/MM/yyyy",
            profile?.Theme ?? "system",
            profile?.DisplayName,
            profile?.PhoneNumber,
            profile?.DateOfBirth,
            avatarUrl);

        return new GetProfileResponse(dto);
    }
}
