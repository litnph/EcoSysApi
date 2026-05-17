namespace PFP.Application.Common.Exceptions;

/// <summary>Thrown when an invariant or business rule is violated (HTTP 422 in the API layer).</summary>
public sealed class BusinessRuleException : Exception
{
    /// <summary>Creates the exception with a user-safe message.</summary>
    public BusinessRuleException(string message) : base(message) { }

    /// <summary>Creates the exception with a user-safe message and an inner error.</summary>
    public BusinessRuleException(string message, Exception innerException) : base(message, innerException) { }
}
