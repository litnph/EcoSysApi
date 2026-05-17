using MediatR;

namespace PFP.Application.Features.Auth.Register;

/// <summary>Registers a new email + password account and bootstraps the personal org / space graph.</summary>
public sealed record RegisterCommand(string Email, string Password, string FullName)
    : IRequest<RegisterResponse>;
