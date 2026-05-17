using System.Security.Claims;

namespace PFP.Application.Common.Models;

/// <summary>Outcome of validating a JWT access token outside the ASP.NET Core bearer middleware.</summary>
public sealed class JwtTokenValidationResult
{
    private JwtTokenValidationResult(bool isValid, ClaimsPrincipal? principal, string? failureReason)
    {
        IsValid = isValid;
        Principal = principal;
        FailureReason = failureReason;
    }

    /// <summary><c>true</c> when the token signature, issuer, audience, and lifetime checks succeed.</summary>
    public bool IsValid { get; }

    /// <summary>Claims principal produced by the handler; <c>null</c> when <see cref="IsValid"/> is <c>false</c>.</summary>
    public ClaimsPrincipal? Principal { get; }

    /// <summary>Human-readable reason when <see cref="IsValid"/> is <c>false</c>.</summary>
    public string? FailureReason { get; }

    /// <summary>Builds a successful validation result.</summary>
    public static JwtTokenValidationResult Success(ClaimsPrincipal principal) =>
        new(true, principal, null);

    /// <summary>Builds a failed validation result.</summary>
    public static JwtTokenValidationResult Failed(string reason) =>
        new(false, null, reason);
}
