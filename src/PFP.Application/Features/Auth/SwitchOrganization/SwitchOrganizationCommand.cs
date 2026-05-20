using MediatR;

namespace PFP.Application.Features.Auth.SwitchOrganization;

/// <summary>
/// Re-issues the access token with <c>org_id</c> set to an organisation the caller belongs to
/// and persists the choice on the current refresh session.
/// </summary>
public sealed record SwitchOrganizationCommand(Guid OrganizationId) : IRequest<SwitchOrganizationResponse>;

/// <summary>New JWT pair scoped to the requested organisation.</summary>
public sealed record SwitchOrganizationResponse(
    Guid UserId,
    Guid OrganizationId,
    Guid SessionId,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
