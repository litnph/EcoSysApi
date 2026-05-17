using MediatR;

namespace PFP.Application.Features.Auth.ForgotPassword;

/// <summary>Requests a password-reset email for the supplied address.</summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResponse>;
