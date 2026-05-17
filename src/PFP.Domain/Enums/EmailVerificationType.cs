namespace PFP.Domain.Enums;

/// <summary>
/// Purpose of a row in <c>USER_EMAIL_VERIFICATIONS</c>.
/// </summary>
public enum EmailVerificationType
{
    /// <summary>
    /// <c>verify_email</c> — initial verification of the address used at registration.
    /// On success, sets <c>USERS.is_email_verified = true</c>.
    /// </summary>
    VerifyEmail = 1,

    /// <summary>
    /// <c>change_email</c> — confirmation of an email-change request.
    /// On success, swaps <c>USERS.email</c> with the new address recorded on the verification row.
    /// </summary>
    ChangeEmail = 2,
}
