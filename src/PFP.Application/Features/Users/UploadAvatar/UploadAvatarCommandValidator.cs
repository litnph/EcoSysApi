using FluentValidation;

namespace PFP.Application.Features.Users.UploadAvatar;

/// <summary>FluentValidation rules for <see cref="UploadAvatarCommand"/>.</summary>
public sealed class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    /// <summary>5 MB upload cap (matches spec §3.5 attachment policy).</summary>
    public const long MaxSizeBytes = 5L * 1024 * 1024;

    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
    };

    /// <summary>Registers field rules.</summary>
    public UploadAvatarCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Avatar must be a JPEG, PNG or WebP image.");
        RuleFor(x => x.SizeBytes).GreaterThan(0).LessThanOrEqualTo(MaxSizeBytes);
        RuleFor(x => x.Content).NotNull();
    }
}
