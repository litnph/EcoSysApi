using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Constants;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Auth;
using PFP.Application.Features.Auth.Common;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Auth.ForgotPassword;

/// <summary>Creates an append-only password-reset row when a local-password account exists.</summary>
public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenHasher _tokenHasher;
    private readonly IAuthEmailDispatcher _emailDispatcher;
    private readonly IClientRequestContext _client;

    /// <summary>Creates the handler.</summary>
    public ForgotPasswordCommandHandler(
        IApplicationDbContext db,
        ITokenHasher tokenHasher,
        IAuthEmailDispatcher emailDispatcher,
        IClientRequestContext client)
    {
        _db = db;
        _tokenHasher = tokenHasher;
        _emailDispatcher = emailDispatcher;
        _client = client;
    }

    /// <inheritdoc/>
    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        const string publicMessage =
            "If an account exists for that email address, password reset instructions have been sent.";

        var email = AuthEmailNormalizer.Normalize(request.Email);

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            return new ForgotPasswordResponse(publicMessage);

        var plain = SecureTokenGenerator.CreateUrlSafe();
        var hash = _tokenHasher.Sha256Hex(plain);
        var expires = DateTime.UtcNow.AddHours(AuthConstants.PasswordResetTokenLifetimeHours);

        _db.UserPasswordResets.Add(
            new UserPasswordReset
            {
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAt = expires,
                RequestIpAddress = _client.IpAddress,
            });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _emailDispatcher.DispatchPasswordReset(user.Email, user.FullName, plain);

        return new ForgotPasswordResponse(publicMessage);
    }
}
