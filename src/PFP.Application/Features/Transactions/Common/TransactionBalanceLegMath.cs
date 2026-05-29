using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Per-type balance sign rules (aligned with create/delete handlers and balance calculator).</summary>
public static class TransactionBalanceLegMath
{
    /// <summary>Applies one transaction leg to a running balance.</summary>
    public static decimal ApplyLeg(decimal running, TransactionType type, decimal amount) =>
        type switch
        {
            TransactionType.Income => running + amount,
            TransactionType.DebtBorrow => running + amount,
            TransactionType.LoanCollect => running + amount,
            TransactionType.Deferred => running + amount,
            TransactionType.Direct => running - amount,
            TransactionType.Split => running - amount,
            TransactionType.DebtRepay => running - amount,
            TransactionType.LoanGive => running - amount,
            TransactionType.Transfer => running + amount,
            TransactionType.BalanceAdjustment => running + amount,
            _ => running,
        };

    /// <summary>Signed change to source balance for a single leg.</summary>
    public static decimal LedgerDelta(TransactionType type, decimal amount) =>
        ApplyLeg(0m, type, amount);

    /// <summary>Net balance change when amount moves from <paramref name="oldAmount"/> to <paramref name="newAmount"/>.</summary>
    public static decimal BalanceChange(TransactionType type, decimal oldAmount, decimal newAmount) =>
        LedgerDelta(type, newAmount) - LedgerDelta(type, oldAmount);
}
