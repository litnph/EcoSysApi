using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PFP.API.Configuration;
using PFP.API.Filters;
using PFP.API.Models;
using PFP.Application.Features.Auth.Login;
using PFP.Application.Features.Auth.Logout;
using PFP.Application.Features.Auth.RefreshToken;

namespace PFP.API.Controllers;

/// <summary>REST surface for authentication (login, refresh, logout).</summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Authenticates with email + password. Rate-limited at 5 attempts / 15 min / IP (spec §6.1).</summary>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.AuthLogin)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken).ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>Rotates refresh material and mints a new access token.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken).ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>Revokes the refresh session bound to the bearer access token.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<LogoutResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LogoutResponse>>> Logout(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LogoutCommand(), cancellationToken).ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }
}

/// <summary>JSON contract for <c>POST /auth/login</c>.</summary>
public sealed class LoginRequest
{
    /// <summary>Account email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Plain-text password.</summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>JSON contract for <c>POST /auth/refresh</c>.</summary>
public sealed class RefreshRequest
{
    /// <summary>Opaque refresh token returned by login/refresh.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}
