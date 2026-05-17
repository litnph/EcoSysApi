namespace PFP.Domain.Enums;

/// <summary>Lifecycle of a <c>FIN_SAVINGS</c> row.</summary>
public enum SavingStatus
{
    /// <summary>Book is active.</summary>
    Active = 1,

    /// <summary>Maturity date reached (fixed-term).</summary>
    Matured = 2,

    /// <summary>Funds withdrawn / book closed.</summary>
    Withdrawn = 3,
}
