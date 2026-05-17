namespace PFP.Application.Features.Auth.ForgotPassword;

/// <summary>Opaque acknowledgement — never reveals whether the account exists (spec §6.1).</summary>
public sealed record ForgotPasswordResponse(string Message);
