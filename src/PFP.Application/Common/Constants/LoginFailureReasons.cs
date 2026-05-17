namespace PFP.Application.Common.Constants;

/// <summary>Machine-readable values stored on <c>USER_LOGIN_ATTEMPTS.failure_reason</c>.</summary>
public static class LoginFailureReasons
{
    public const string InvalidPassword = "invalid_password";
    public const string AccountLocked = "account_locked";
    public const string UserNotFound = "user_not_found";
    public const string MissingPassword = "missing_password";
}
