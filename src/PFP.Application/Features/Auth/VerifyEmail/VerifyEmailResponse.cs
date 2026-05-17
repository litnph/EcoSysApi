namespace PFP.Application.Features.Auth.VerifyEmail;

/// <summary>Result of redeeming an email verification token.</summary>
public sealed record VerifyEmailResponse(bool EmailVerified, string Email);
