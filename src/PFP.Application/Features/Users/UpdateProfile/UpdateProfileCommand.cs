using MediatR;
using PFP.Application.Features.Users.Common;

namespace PFP.Application.Features.Users.UpdateProfile;

/// <summary>Updates the editable fields of <c>USERS</c> + <c>USER_PROFILES</c>.</summary>
public sealed record UpdateProfileCommand(
    string FullName,
    string? DisplayName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string LanguageCode,
    string Timezone,
    string DateFormat,
    string Theme) : IRequest<UpdateProfileResponse>;

/// <summary>Response wrapper.</summary>
public sealed record UpdateProfileResponse(UserProfileDto Profile);
