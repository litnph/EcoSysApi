using MediatR;
using PFP.Application.Features.Users.Common;

namespace PFP.Application.Features.Users.GetProfile;

/// <summary>Returns the full <c>USER_PROFILES</c> sidecar for the authenticated user.</summary>
public sealed record GetProfileQuery() : IRequest<GetProfileResponse>;

/// <summary>Response wrapper.</summary>
public sealed record GetProfileResponse(UserProfileDto Profile);
