using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Users.UploadAvatar;

/// <summary>
/// Streams the avatar to object storage, marks previous active rows inactive, then inserts a
/// fresh <see cref="UserAvatarUpload"/> row pointing at the new key.
/// </summary>
public sealed class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, UploadAvatarResponse>
{
    private const string StoragePrefix = "avatars";
    private const int SignedUrlMinutes = 60 * 24 * 7; // 7-day download URL.

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storage;

    /// <summary>Creates the handler.</summary>
    public UploadAvatarCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IStorageService storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    /// <inheritdoc/>
    public async Task<UploadAvatarResponse> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var extension = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = request.ContentType switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".jpg",
            };

        var key = $"{StoragePrefix}/{userId:N}/{Guid.NewGuid():N}{extension}";

        var storedKey = await _storage
            .UploadAsync(request.Content, key, request.ContentType, cancellationToken)
            .ConfigureAwait(false);

        var publicUrl = await _storage
            .GetSignedUrlAsync(storedKey, TimeSpan.FromMinutes(SignedUrlMinutes), cancellationToken)
            .ConfigureAwait(false);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var prior = await _db.UserAvatarUploads
            .Where(a => a.UserId == userId && a.IsActive)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (var p in prior)
            p.IsActive = false;

        var avatar = new UserAvatarUpload
        {
            UserId = userId,
            StorageKey = storedKey,
            StorageUrl = publicUrl,
            ContentType = request.ContentType,
            SizeBytes = request.SizeBytes,
            IsActive = true,
        };
        _db.UserAvatarUploads.Add(avatar);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new UploadAvatarResponse(avatar.Id, avatar.StorageKey, publicUrl);
    }
}
