namespace PFP.Domain.Enums;

/// <summary>Lifecycle status of a <c>fin_debt_records</c> row.</summary>
public enum DebtStatus
{
    /// <summary><c>active</c> — outstanding or partially settled.</summary>
    Active = 1,

    /// <summary><c>completed</c> — <c>remaining_amount</c> reached zero.</summary>
    Completed = 2,

    /// <summary><c>cancelled</c> — manually cancelled / soft-deleted as erroneous entry.</summary>
    Cancelled = 3,
}
