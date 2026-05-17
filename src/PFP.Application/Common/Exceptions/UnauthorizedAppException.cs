namespace PFP.Application.Common.Exceptions;

/// <summary>Thrown when credentials are invalid or the caller is not allowed to perform an auth operation (HTTP 401).</summary>
public sealed class UnauthorizedAppException : Exception
{
    /// <summary>Creates the exception with a user-safe message.</summary>
    public UnauthorizedAppException(string message) : base(message) { }
}
