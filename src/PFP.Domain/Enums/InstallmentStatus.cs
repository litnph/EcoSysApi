namespace PFP.Domain.Enums;

/// <summary>Lifecycle of a <c>FIN_INSTALLMENT_PLANS</c> row.</summary>
public enum InstallmentStatus
{
    /// <summary><c>active</c> — plan is in progress.</summary>
    Active = 1,

    /// <summary><c>completed</c> — every installment has been paid.</summary>
    Completed = 2,

    /// <summary><c>cancelled</c> — plan was cancelled before completion.</summary>
    Cancelled = 3,
}
