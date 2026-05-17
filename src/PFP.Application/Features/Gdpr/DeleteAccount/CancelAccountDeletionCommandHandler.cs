using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Gdpr.DeleteAccount;

/// <summary>Sets deletion request to <see cref="DeletionRequestStatus.Cancelled"/> when allowed.</summary>
public sealed class CancelAccountDeletionCommandHandler : IRequestHandler<CancelAccountDeletionCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    /// <summary>Creates the handler.</summary>
    public CancelAccountDeletionCommandHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(CancelAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        if (_current.UserId is not Guid userId)
            throw new UnauthorizedAppException("Authentication is required.");

        var now = DateTime.UtcNow;
        var row = await _db.UserDeletionRequests
            .Where(r =>
                r.UserId == userId
                && (r.Status == DeletionRequestStatus.Pending || r.Status == DeletionRequestStatus.Confirmed))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
            throw new NotFoundException("No active account deletion request was found.");

        if (row.Status == DeletionRequestStatus.Confirmed
            && row.ScheduledExecutionAt is { } due
            && due <= now)
        {
            throw new BusinessRuleException("The grace period has ended; this deletion can no longer be cancelled online.");
        }

        row.Status = DeletionRequestStatus.Cancelled;
        row.CancelledAt = now;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
