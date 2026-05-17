using MediatR;

namespace PFP.Application.Features.Auth.ResetPassword;

/// <summary>Completes the forgot-password flow with a one-time token.</summary>
public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest<ResetPasswordResponse>;
