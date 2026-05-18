using MediatR;

namespace PFP.Application.Features.Users.UploadAvatar;

/// <summary>
/// Uploads a new avatar image, marks any prior avatar inactive, and returns the public URL.
/// <para>
/// The raw bytes are streamed straight into Cloudflare R2 via <c>IStorageService</c>; only the
/// metadata (key, URL, content type, size) is persisted in <c>USER_AVATAR_UPLOADS</c>.
/// </para>
/// </summary>
public sealed record UploadAvatarCommand(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes) : IRequest<UploadAvatarResponse>;

/// <summary>Response containing the URL of the freshly activated avatar.</summary>
public sealed record UploadAvatarResponse(Guid AvatarId, string StorageKey, string StorageUrl);
