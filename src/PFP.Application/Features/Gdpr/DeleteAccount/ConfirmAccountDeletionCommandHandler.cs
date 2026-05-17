using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Constants;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Gdpr.DeleteAccount;

/// <summary>Marks deletion as confirmed and starts the 30-day grace timer.</summary>
public sealed class ConfirmAccountDeletionCommandHandler : IRequestHandler<ConfirmAccountDeletionCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenHasher _tokenHasher;
    private readonly IAuthEmailDispatcher _email;

    /// <summary>Creates the handler.</summary>
    public ConfirmAccountDeletionCommandHandler(
        IApplicationDbContext db,
        ITokenHasher tokenHasher,
        IAuthEmailDispatcher email)
    {
        _db = db;
        _tokenHasher = tokenHasher;
        _email = email;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(ConfirmAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenHasher.Sha256Hex(request.Token);
        var now = DateTime.UtcNow;

        var row = await _db.UserDeletionRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(
                r =>
                    r.ConfirmationTokenHash == hash
                    && r.Status == DeletionRequestStatus.Pending
                    && r.ConfirmationTokenExpiresAt > now,
                cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
            throw new UnauthorizedAppException("This confirmation link is invalid or has expired.");

        var scheduled = now.AddDays(AuthConstants.AccountDeletionGracePeriodDays);
        row.Status = DeletionRequestStatus.Confirmed;
        row.ConfirmedAt = now;
        row.ScheduledExecutionAt = scheduled;
        row.ConfirmationTokenHash = null;
        row.ConfirmationTokenExpiresAt = null;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _email.DispatchAccountDeletionGracePeriodNotice(row.User.Email, row.User.FullName, scheduled);

        return Unit.Value;
    }
}
