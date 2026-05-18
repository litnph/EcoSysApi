using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Users.ChangePassword;

/// <summary>
/// Verifies the current password, persists the new bcrypt hash, and revokes every refresh token
/// except the one carrying the request (spec §6.1).
/// </summary>
public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>Creates the handler.</summary>
    public ChangePasswordCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IPasswordHasher passwordHasher)
    {
        _db = db;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc/>
    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;
        var currentSession = _currentUser.SessionId;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            throw new NotFoundException("User was not found.");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new BusinessRuleException("This account has no local password set. Use the password-reset flow instead.");

        if (!_passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
            throw new UnauthorizedAppException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        var now = DateTime.UtcNow;
        var sessions = await _db.UserSessions
            .Where(s => s.UserId == userId && s.RevokedAt == null && s.Id != currentSession)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var s in sessions)
            s.RevokedAt = now;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ChangePasswordResponse(sessions.Count);
    }
}
