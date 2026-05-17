namespace PFP.Domain.Enums;

/// <summary>
/// Direction of money flow for a <c>FIN_CATEGORIES</c> row (Sprint 2 — expense / income / transfer).
/// </summary>
public enum CategoryKind
{
    /// <summary><c>expense</c> — outgoing categories: food, rent, utilities, …</summary>
    Expense = 1,

    /// <summary><c>income</c> — incoming categories: salary, bonus, dividend, …</summary>
    Income = 2,

    /// <summary><c>transfer</c> — categories used for account-to-account transfers.</summary>
    Transfer = 3,
}
