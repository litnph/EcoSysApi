namespace PFP.Application.Common.Exceptions;

/// <summary>Thrown when a requested aggregate or resource does not exist (HTTP 404 in the API layer).</summary>
public sealed class NotFoundException : Exception
{
    /// <summary>Creates the exception with a user-safe message.</summary>
    public NotFoundException(string message) : base(message) { }
}
