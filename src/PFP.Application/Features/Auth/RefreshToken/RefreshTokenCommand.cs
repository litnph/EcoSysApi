using MediatR;

namespace PFP.Application.Features.Auth.RefreshToken;

/// <summary>Exchanges a valid refresh token for a fresh JWT access token (and rotated refresh material).</summary>
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;
