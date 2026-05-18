namespace PFP.Domain.Exceptions;

/// <summary>
/// Base class for invariant / aggregate-level errors raised inside the Domain layer.
/// <para>
/// Domain exceptions describe a violation of a business invariant on the entity itself
/// (e.g. <see cref="InsufficientBalanceException"/>). The Application layer catches them
/// and rethrows the appropriate <c>BusinessRuleException</c> or maps them directly via
/// the API exception middleware (HTTP 422).
/// </para>
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>Creates the exception with a user-safe message.</summary>
    protected DomainException(string message) : base(message) { }

    /// <summary>Creates the exception with a user-safe message and an inner cause.</summary>
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}
