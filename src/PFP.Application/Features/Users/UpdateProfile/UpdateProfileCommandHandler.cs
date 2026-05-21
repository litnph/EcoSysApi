using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Users.Common;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Users.UpdateProfile;

/// <summary>Mutates the caller's <c>USERS.FullName</c> and the <c>USER_PROFILES</c> sidecar (creates it on first edit).</summary>
public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public UpdateProfileCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<UpdateProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            throw new NotFoundException("User was not found.");

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            profile = new UserProfile { UserId = userId };
            _db.UserProfiles.Add(profile);
        }

        user.FullName = request.FullName.Trim();

        profile.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim();
        profile.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        profile.DateOfBirth = request.DateOfBirth;
        profile.LanguageCode = request.LanguageCode.Trim();
        profile.Timezone = request.Timezone.Trim();
        profile.DateFormat = request.DateFormat.Trim();
        profile.Theme = request.Theme.Trim().ToLowerInvariant();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            profile.LanguageCode,
            profile.Timezone,
            profile.DateFormat,
            profile.Theme,
            profile.DisplayName,
            profile.PhoneNumber,
            profile.DateOfBirth,
            profile.AvatarUrl);

        return new UpdateProfileResponse(dto);
    }
}
