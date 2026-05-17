using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Auth.ResetPassword;

/// <summary>Redeems a password-reset token, updates the bcrypt hash, and revokes active refresh sessions.</summary>
public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenHasher _tokenHasher;
    private readonly IClientRequestContext _client;

    /// <summary>Creates the handler.</summary>
    public ResetPasswordCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        ITokenHasher tokenHasher,
        IClientRequestContext client)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenHasher = tokenHasher;
        _client = client;
    }

    /// <inheritdoc/>
    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenHasher.Sha256Hex(request.Token);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var reset = await _db.UserPasswordResets
            .FirstOrDefaultAsync(
                r => r.TokenHash == hash && r.UsedAt == null && r.ExpiresAt > DateTime.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);

        if (reset is null)
            throw new NotFoundException("The password reset link is invalid or has expired.");

        var user = await _db.Users.FirstAsync(u => u.Id == reset.UserId, cancellationToken).ConfigureAwait(false);
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        var now = DateTime.UtcNow;
        reset.UsedAt = now;
        reset.UsedIpAddress = _client.IpAddress;

        var sessions = await _db.UserSessions
            .Where(s => s.UserId == user.Id && s.RevokedAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var session in sessions)
            session.RevokedAt = now;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new ResetPasswordResponse(true);
    }
}
