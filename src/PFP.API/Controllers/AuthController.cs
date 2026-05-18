using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using PFP.API.Configuration;
using PFP.API.Filters;
using PFP.API.Models;
using PFP.Application.Features.Auth.ForgotPassword;
using PFP.Application.Features.Auth.Google;
using PFP.Application.Features.Auth.Login;
using PFP.Application.Features.Auth.Logout;
using PFP.Application.Features.Auth.RefreshToken;
using PFP.Application.Features.Auth.Register;
using PFP.Application.Features.Auth.ResetPassword;
using PFP.Application.Features.Auth.VerifyEmail;
using PFP.Infrastructure.Identity;

namespace PFP.API.Controllers;

/// <summary>REST surface for authentication flows (spec §5.2).</summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly GoogleOAuthOptions _google;
    private readonly FrontendOptions _frontend;

    /// <summary>Creates the controller.</summary>
    public AuthController(
        IMediator mediator,
        IOptions<GoogleOAuthOptions> googleOptions,
        IOptions<FrontendOptions> frontendOptions)
    {
        _mediator = mediator;
        _google = googleOptions.Value;
        _frontend = frontendOptions.Value;
    }

    /// <summary>Starts the Google OAuth 2.0 sign-in flow (redirects to Google).</summary>
    [HttpGet("google")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult Google([FromQuery] string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(_google.ClientId) || string.IsNullOrWhiteSpace(_google.ClientSecret))
            return NotFound(ApiResults.Fail<object>(new { code = "not_configured", messages = new[] { "Google OAuth is not configured." } }));

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback)),
        };

        if (!string.IsNullOrWhiteSpace(returnUrl))
            properties.Items["return_url"] = returnUrl;

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>Google OAuth callback — exchanges the external cookie for API JWT + refresh tokens (spec §5.2).</summary>
    [HttpGet("google/callback")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> GoogleCallback(CancellationToken cancellationToken)
    {
        var auth = await HttpContext.AuthenticateAsync(GoogleAuthConstants.ExternalCookieScheme).ConfigureAwait(false);
        if (!auth.Succeeded || auth.Principal is null)
            return Unauthorized(ApiResults.Fail<LoginResponse>(new { code = "unauthorized", messages = new[] { "Google sign-in failed." } }));

        var email = auth.Principal.FindFirstValue(ClaimTypes.Email);
        var sub = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = auth.Principal.FindFirstValue(ClaimTypes.Name)
            ?? auth.Principal.FindFirstValue("name")
            ?? email;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sub))
            return BadRequest(ApiResults.Fail<LoginResponse>(new { code = "invalid_claims", messages = new[] { "Google did not return the required email or subject claims." } }));

        await HttpContext.SignOutAsync(GoogleAuthConstants.ExternalCookieScheme).ConfigureAwait(false);

        var result = await _mediator.Send(new GoogleLoginCommand(email, sub, name!), cancellationToken).ConfigureAwait(false);

        var returnUrl = "/";
        if (auth.Properties?.Items.TryGetValue("return_url", out var storedReturn) == true
            && !string.IsNullOrWhiteSpace(storedReturn))
        {
            returnUrl = storedReturn;
        }

        var locale = string.IsNullOrWhiteSpace(_frontend.DefaultLocale) ? "vi" : _frontend.DefaultLocale.Trim();
        var feBase = (_frontend.BaseUrl ?? "http://localhost:3000").TrimEnd('/');
        var callback = new UriBuilder($"{feBase}/{locale}/auth/callback")
        {
            Query = string.Join(
                "&",
                new[]
                {
                    $"access_token={Uri.EscapeDataString(result.AccessToken)}",
                    $"refresh_token={Uri.EscapeDataString(result.RefreshToken)}",
                    $"returnUrl={Uri.EscapeDataString(returnUrl)}",
                }),
        };

        return Redirect(callback.Uri.ToString());
    }

    /// <summary>Registers a new email + password account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
                new RegisterCommand(request.Email, request.Password, request.FullName),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }

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

    /// <summary>Queues a password-reset email when the account exists.</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<ForgotPasswordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ForgotPasswordResponse>>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand(request.Email), cancellationToken).ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>Sets a new password from a reset token.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<ResetPasswordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ResetPasswordResponse>>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ResetPasswordCommand(request.Token, request.NewPassword), cancellationToken).ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }

    /// <summary>Marks the user's email as verified.</summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<VerifyEmailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<VerifyEmailResponse>>> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new VerifyEmailCommand(request.Token), cancellationToken).ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }
}

/// <summary>JSON contract for <c>POST /auth/register</c>.</summary>
public sealed class RegisterRequest
{
    /// <summary>Primary sign-in email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Plain-text password (never persisted).</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Display name used for the personal organisation title.</summary>
    public string FullName { get; set; } = string.Empty;
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
    /// <summary>Opaque refresh token returned by register/login/refresh.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>JSON contract for <c>POST /auth/forgot-password</c>.</summary>
public sealed class ForgotPasswordRequest
{
    /// <summary>Account email.</summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>JSON contract for <c>POST /auth/reset-password</c>.</summary>
public sealed class ResetPasswordRequest
{
    /// <summary>Single-use token from the reset email.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>New password.</summary>
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>JSON contract for <c>POST /auth/verify-email</c>.</summary>
public sealed class VerifyEmailRequest
{
    /// <summary>Single-use token from the verification email.</summary>
    public string Token { get; set; } = string.Empty;
}
