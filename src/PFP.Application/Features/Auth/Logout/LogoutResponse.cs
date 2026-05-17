namespace PFP.Application.Features.Auth.Logout;

/// <summary>Acknowledgement payload for session revocation.</summary>
public sealed record LogoutResponse(bool Success);
