using MediatR;

namespace PFP.Application.Features.Auth.Login;

/// <summary>Authenticates an email + password user and issues a new refresh session.</summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
