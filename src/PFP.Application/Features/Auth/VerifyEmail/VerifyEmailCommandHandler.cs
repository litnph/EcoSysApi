using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Auth.VerifyEmail;

/// <summary>Marks <c>USERS.is_email_verified</c> when a valid verification token is presented.</summary>
public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenHasher _tokenHasher;

    /// <summary>Creates the handler.</summary>
    public VerifyEmailCommandHandler(IApplicationDbContext db, ITokenHasher tokenHasher)
    {
        _db = db;
        _tokenHasher = tokenHasher;
    }

    /// <inheritdoc/>
    public async Task<VerifyEmailResponse> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenHasher.Sha256Hex(request.Token);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var row = await _db.UserEmailVerifications
            .FirstOrDefaultAsync(
                v => v.TokenHash == hash
                     && v.Type == EmailVerificationType.VerifyEmail
                     && v.VerifiedAt == null
                     && v.ExpiresAt > DateTime.UtcNow,
                cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
            throw new NotFoundException("The email verification link is invalid or has expired.");

        var user = await _db.Users.FirstAsync(u => u.Id == row.UserId, cancellationToken).ConfigureAwait(false);
        user.IsEmailVerified = true;
        row.VerifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new VerifyEmailResponse(true, user.Email);
    }
}
