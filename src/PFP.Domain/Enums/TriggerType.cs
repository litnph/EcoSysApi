namespace PFP.Domain.Enums;

/// <summary>When an <see cref="Entities.AutomationRule"/> is evaluated by the hourly job.</summary>
public enum TriggerType
{
    /// <summary>Fires when new income is detected for the module (job window).</summary>
    IncomeReceived = 1,

    /// <summary>Fires when a billing cycle transitions to closed recently.</summary>
    StatementDate = 2,

    /// <summary>Fires when UTC calendar date matches <c>TriggerValue</c> (<c>yyyy-MM-dd</c>).</summary>
    FixedDate = 3,

    /// <summary>Fires when an active debt has <see cref="Finance.FinDebtRecord.DueDate"/> due today.</summary>
    DebtDue = 4,
}
