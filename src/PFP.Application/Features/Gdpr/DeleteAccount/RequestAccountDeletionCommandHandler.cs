using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Constants;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Auth.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Gdpr.DeleteAccount;

/// <summary>Creates deletion request row and sends confirmation email.</summary>
public sealed class RequestAccountDeletionCommandHandler : IRequestHandler<RequestAccountDeletionCommand, RequestAccountDeletionResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly ITokenHasher _tokenHasher;
    private readonly IAuthEmailDispatcher _email;

    /// <summary>Creates the handler.</summary>
    public RequestAccountDeletionCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService current,
        ITokenHasher tokenHasher,
        IAuthEmailDispatcher email)
    {
        _db = db;
        _current = current;
        _tokenHasher = tokenHasher;
        _email = email;
    }

    /// <inheritdoc/>
    public async Task<RequestAccountDeletionResponse> Handle(
        RequestAccountDeletionCommand request,
        CancellationToken cancellationToken)
    {
        if (_current.UserId is not Guid userId)
            throw new UnauthorizedAppException("Authentication is required.");

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
            throw new NotFoundException("User was not found.");

        var plain = SecureTokenGenerator.CreateUrlSafe();
        var hash = _tokenHasher.Sha256Hex(plain);
        var expires = DateTime.UtcNow.AddHours(AuthConstants.AccountDeletionConfirmationTokenLifetimeHours);

        var row = new UserDeletionRequest
        {
            UserId = userId,
            Status = DeletionRequestStatus.Pending,
            Reason = request.Reason,
            ConfirmationTokenHash = hash,
            ConfirmationTokenExpiresAt = expires,
        };

        _db.UserDeletionRequests.Add(row);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _email.DispatchAccountDeletionConfirmation(user.Email, user.FullName, row.Id, plain);

        return new RequestAccountDeletionResponse(row.Id);
    }
}
