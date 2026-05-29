namespace PFP.Domain.Enums;

/// <summary>Workflow status on <c>FIN_TRANSACTIONS.status</c>.</summary>
public enum TxnStatus
{
    /// <summary><c>new</c> — created, not yet included in a closed monthly report.</summary>
    New = 1,

    /// <summary><c>transferred_to_installment</c> — original deferred txn converted to an installment plan.</summary>
    TransferredToInstallment = 2,

    /// <summary><c>completed</c> — month closed; transaction frozen in monthly report.</summary>
    Completed = 3,

    /// <summary><c>cancelled</c> — voided.</summary>
    Cancelled = 4,
}
