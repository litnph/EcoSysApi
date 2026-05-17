using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Auth.Logout;

/// <summary>Soft-revokes the caller's refresh-token session row.</summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public LogoutCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.SessionId is null)
            throw new UnauthorizedAppException("Authentication is required to log out.");

        var sessionId = _currentUser.SessionId.Value;

        var session = await _db.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            .ConfigureAwait(false);

        if (session is null || session.RevokedAt is not null)
            return new LogoutResponse(true);

        session.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new LogoutResponse(true);
    }
}
