namespace PFP.Domain.Enums;

/// <summary>
/// All recognised <c>FIN_TRANSACTIONS.type</c> values.
/// <para>
/// The integer values are stable and used only for in-process safety; the database column
/// stores the snake_case string shown in each member's documentation
/// (mapping is configured in the Infrastructure layer via EF Core <c>HasConversion</c>).
/// </para>
/// <para>
/// Each transaction type carries a specific side-effect on <c>FIN_SOURCES.balance</c>
/// or on the debt / split / installment subsystems — see
/// <c>BUSINESS LOGIC — CRITICAL FLOWS</c> in the backend spec.
/// </para>
/// </summary>
public enum TransactionType
{
    /// <summary><c>direct</c> — straightforward payment. Decreases <c>FIN_SOURCES.balance</c>.</summary>
    Direct = 1,

    /// <summary>
    /// <c>deferred</c> — credit-card spend (use now, pay later).
    /// Increases the credit-card source balance and is attached to a <c>billing_cycle_id</c>.
    /// </summary>
    Deferred = 2,

    /// <summary><c>income</c> — incoming funds (salary, bonus, …). Increases <c>FIN_SOURCES.balance</c>.</summary>
    Income = 3,

    /// <summary>
    /// <c>transfer</c> — internal move between two of the user's own sources.
    /// Persisted as two linked rows (source side and destination side) joined by <c>ref_txn_id</c>.
    /// </summary>
    Transfer = 4,

    /// <summary>
    /// <c>split</c> — user paid on behalf of others who will reimburse later.
    /// Spawns one row in <c>FIN_TXN_SPLITS</c> per participant.
    /// </summary>
    Split = 5,

    /// <summary>
    /// <c>debt_borrow</c> — user borrowed money from someone else.
    /// Creates a <c>FIN_DEBT_RECORDS</c> row with <see cref="DebtDirection.Borrowed"/>.
    /// </summary>
    DebtBorrow = 6,

    /// <summary>
    /// <c>debt_repay</c> — user paid back part/all of a borrowed debt.
    /// Appends a <c>FIN_DEBT_TRANSACTIONS</c> row and decreases <c>remaining_amount</c>.
    /// </summary>
    DebtRepay = 7,

    /// <summary>
    /// <c>loan_give</c> — user lent money to someone else.
    /// Creates a <c>FIN_DEBT_RECORDS</c> row with <see cref="DebtDirection.Lent"/>.
    /// </summary>
    LoanGive = 8,

    /// <summary>
    /// <c>loan_collect</c> — user collected part/all of a previously lent amount.
    /// Appends a <c>FIN_DEBT_TRANSACTIONS</c> row and decreases <c>remaining_amount</c>.
    /// </summary>
    LoanCollect = 9,

    /// <summary>
    /// <c>reversal</c> — system-managed compensation row produced by the soft-delete-and-reverse flow.
    /// Created automatically when an existing transaction is deleted; carries a negative <c>amount</c>
    /// and points to the original row through <c>ref_txn_id</c>. Handlers MUST NOT create
    /// reversal rows directly — they are emitted only by the delete pipeline.
    /// </summary>
    Reversal = 10,

    /// <summary>
    /// <c>balance_adjustment</c> — manual reconciliation delta on an asset source.
    /// <c>amount</c> is signed (positive increases balance, negative decreases).
    /// Excluded from monthly income/expense reports.
    /// </summary>
    BalanceAdjustment = 11,
}
