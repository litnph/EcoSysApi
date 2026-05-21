using MediatR;

namespace PFP.Application.Features.Users.UploadAvatar;

/// <summary>
/// Uploads a new avatar image and stores the public URL on <c>user_profiles.avatar_url</c>.
/// </summary>
public sealed record UploadAvatarCommand(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes) : IRequest<UploadAvatarResponse>;

/// <summary>Response containing the URL of the uploaded avatar.</summary>
public sealed record UploadAvatarResponse(string AvatarUrl);
