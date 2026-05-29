using PFP.Application.Common;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.Common;

/// <summary>Builds a running balance ledger ordered by transaction date.</summary>
public static class SourceBalanceLedgerBuilder
{
    /// <summary>Computes ledger rows and the balance implied by opening + transactions.</summary>
    public static (IReadOnlyList<SourceBalanceLedgerEntryDto> Entries, decimal ComputedBalance) Build(
        FinSource source,
        IReadOnlyList<FinTransaction> transactions)
    {
        var entries = new List<SourceBalanceLedgerEntryDto>();
        var running = source.InitialBalance ?? 0m;
        var opening = source.InitialBalance ?? 0m;

        if (opening != 0m)
        {
            var openingDate = DateOnly.FromDateTime(source.CreatedAt);
            entries.Add(new SourceBalanceLedgerEntryDto(
                "opening",
                null,
                null,
                openingDate,
                "Số dư ban đầu",
                CurrencyUnits.ToWhole(opening),
                CurrencyUnits.ToWhole(running)));
        }

        var legs = transactions
            .Where(t => t.Status != TxnStatus.Cancelled
                        && t.Type != TransactionType.Reversal
                        && !t.IsDeleted)
            .OrderBy(t => t.TxnDate)
            .ThenBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .ToList();

        foreach (var txn in legs)
        {
            var delta = TransactionBalanceLegMath.LedgerDelta(txn.Type, txn.Amount);
            running = TransactionBalanceLegMath.ApplyLeg(running, txn.Type, txn.Amount);
            var typeKey = txn.Type switch
            {
                TransactionType.BalanceAdjustment => "balance_adjustment",
                TransactionType.DebtBorrow => "debt_borrow",
                TransactionType.DebtRepay => "debt_repay",
                TransactionType.LoanGive => "loan_give",
                TransactionType.LoanCollect => "loan_collect",
                _ => txn.Type.ToString().ToLowerInvariant(),
            };

            entries.Add(new SourceBalanceLedgerEntryDto(
                "transaction",
                txn.Id,
                typeKey,
                txn.TxnDate,
                string.IsNullOrWhiteSpace(txn.Description)
                    ? typeKey
                    : txn.Description.Trim(),
                CurrencyUnits.ToWhole(delta),
                CurrencyUnits.ToWhole(decimal.Round(running, 2, MidpointRounding.ToEven))));
        }

        return (entries, decimal.Round(running, 2, MidpointRounding.ToEven));
    }
}
