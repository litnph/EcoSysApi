using MediatR;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Auth.RefreshToken;

/// <summary>Delegates refresh rotation to <see cref="IJwtTokenService.ExchangeRefreshTokenAsync"/> (includes EF retry strategy).</summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>Creates the handler.</summary>
    public RefreshTokenCommandHandler(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    /// <inheritdoc/>
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _jwtTokenService
            .ExchangeRefreshTokenAsync(request.RefreshToken, cancellationToken)
            .ConfigureAwait(false);

        return new RefreshTokenResponse(
            result.UserId,
            result.OrganizationId,
            result.SessionId,
            result.AccessToken,
            result.PlainRefreshToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshTokenExpiresAtUtc);
    }
}
