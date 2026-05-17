namespace PFP.Domain.Enums;

/// <summary>Status of a single <c>FIN_INSTALLMENT_PAYS</c> row.</summary>
public enum InstallmentPayStatus
{
    /// <summary><c>upcoming</c> — due date is in the future.</summary>
    Upcoming = 1,

    /// <summary><c>due</c> — due date has been reached or passed (current period).</summary>
    Due = 2,

    /// <summary><c>paid</c> — installment settled.</summary>
    Paid = 3,

    /// <summary><c>overdue</c> — due date passed without payment.</summary>
    Overdue = 4,
}
