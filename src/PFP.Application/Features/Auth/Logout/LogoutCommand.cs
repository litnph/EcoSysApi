using MediatR;

namespace PFP.Application.Features.Auth.Logout;

/// <summary>Revokes the refresh session associated with the current JWT.</summary>
public sealed record LogoutCommand : IRequest<LogoutResponse>;
