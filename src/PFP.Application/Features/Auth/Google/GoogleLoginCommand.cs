using MediatR;
using PFP.Application.Features.Auth.Login;

namespace PFP.Application.Features.Auth.Google;

/// <summary>Completes Google OAuth using claims returned by Google (called after the external cookie sign-in).</summary>
public sealed record GoogleLoginCommand(string Email, string GoogleSubject, string FullName) : IRequest<LoginResponse>;
