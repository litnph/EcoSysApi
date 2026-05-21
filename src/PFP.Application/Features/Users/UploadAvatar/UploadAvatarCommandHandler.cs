using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Users.UploadAvatar;

/// <summary>
/// Streams the avatar to object storage and persists the public URL on <see cref="UserProfile"/>.
/// </summary>
public sealed class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, UploadAvatarResponse>
{
    private const string StoragePrefix = "avatars";
    private const int SignedUrlMinutes = 60 * 24 * 7;

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

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            profile = new UserProfile { UserId = userId };
            _db.UserProfiles.Add(profile);
        }

        profile.AvatarUrl = publicUrl;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new UploadAvatarResponse(publicUrl);
    }
}
