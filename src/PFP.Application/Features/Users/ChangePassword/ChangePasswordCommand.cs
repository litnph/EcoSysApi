using MediatR;

namespace PFP.Application.Features.Users.ChangePassword;

/// <summary>Updates the caller's bcrypt password hash. Requires the current password for confirmation.</summary>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<ChangePasswordResponse>;

/// <summary>Response indicating that all other sessions have been revoked (per spec §6.1).</summary>
public sealed record ChangePasswordResponse(int RevokedSessions);
