using MediatR;

namespace PFP.Application.Features.Auth.VerifyEmail;

/// <summary>Confirms ownership of the email address using the signed-up verification token.</summary>
public sealed record VerifyEmailCommand(string Token) : IRequest<VerifyEmailResponse>;
