namespace PFP.Application.Common.Exceptions;

/// <summary>
/// Thrown when the caller is authenticated but lacks the role / membership required to
/// perform the operation (HTTP 403 — spec §6.3).
/// <para>
/// Distinct from <see cref="UnauthorizedAppException"/>, which represents missing or invalid
/// credentials (HTTP 401). Use <see cref="ForbiddenException"/> when a JWT was accepted but
/// the user is not allowed to act in this organisation / space / module.
/// </para>
/// </summary>
public sealed class ForbiddenException : Exception
{
    /// <summary>Creates the exception with a user-safe message.</summary>
    public ForbiddenException(string message) : base(message) { }

    /// <summary>Creates the exception with a user-safe message and inner cause.</summary>
    public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}
