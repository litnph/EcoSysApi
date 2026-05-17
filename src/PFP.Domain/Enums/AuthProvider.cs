namespace PFP.Domain.Enums;

/// <summary>
/// Authentication provider linked to a user account.
/// Stored in <c>USER_AUTH_PROVIDERS.provider</c>.
/// <para>
/// A single user may have several active providers (e.g. email login + Google OAuth);
/// at least one provider must remain active at all times.
/// </para>
/// </summary>
public enum AuthProvider
{
    /// <summary><c>email</c> — local email + bcrypt password.</summary>
    Email = 1,

    /// <summary><c>google</c> — Google OAuth 2.0; <c>provider_user_id</c> stores the Google <c>sub</c> claim.</summary>
    Google = 2,

    /// <summary><c>apple</c> — Sign in with Apple; <c>provider_user_id</c> stores the Apple user identifier.</summary>
    Apple = 3,
}
