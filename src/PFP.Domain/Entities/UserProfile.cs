namespace PFP.Domain.Entities;

/// <summary>
/// 1-1 sidecar of <see cref="User"/> holding personalisation and locale settings.
/// Maps to <c>USER_PROFILES</c>.
/// <para>
/// Defaults match the Vietnamese-first MVP launch (per registration spec §4.1):
/// language <c>vi</c>, timezone <c>Asia/Ho_Chi_Minh</c>, date format <c>dd/MM/yyyy</c>, theme <c>system</c>.
/// </para>
/// <para>
/// The 1-1 relationship is enforced by a unique index on <see cref="UserId"/> at the EF configuration level.
/// </para>
/// </summary>
public sealed class UserProfile : BaseEntity
{
    /// <summary>FK to <see cref="User"/> (unique — 1-1 relationship).</summary>
    public Guid UserId { get; set; }

    /// <summary>BCP-47 locale code; FK to <see cref="Locale.Code"/>. Default: <c>vi</c>.</summary>
    public string LanguageCode { get; set; } = "vi";

    /// <summary>IANA timezone identifier. Default: <c>Asia/Ho_Chi_Minh</c>.</summary>
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

    /// <summary>Preferred date format token, e.g. <c>dd/MM/yyyy</c>, <c>yyyy-MM-dd</c>.</summary>
    public string DateFormat { get; set; } = "dd/MM/yyyy";

    /// <summary>UI theme preference: <c>light</c> | <c>dark</c> | <c>system</c>.</summary>
    public string Theme { get; set; } = "system";

    /// <summary>Optional display-name override (otherwise <see cref="User.FullName"/> is shown).</summary>
    public string? DisplayName { get; set; }

    /// <summary>Optional phone number in E.164 format (no validation in Domain).</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Optional date of birth (no time component).</summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Public URL of the user's avatar image (object storage).</summary>
    public string? AvatarUrl { get; set; }

    // ---- Navigation ----

    public User User { get; set; } = null!;

    public Locale? Language { get; set; }
}
