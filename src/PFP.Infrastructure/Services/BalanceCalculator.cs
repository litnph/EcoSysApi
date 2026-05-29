using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Services;

/// <summary>
/// Default <see cref="IBalanceCalculator"/> implementation. Walks every non-deleted, non-reversal
/// <see cref="FinTransaction"/> attached to a source and derives the running balance via the
/// per-type sign rules used by the create handlers (spec §3.6 / §4.6).
/// <para>
/// Sign rules (matching <c>CreateTransactionCommandHandler</c>):
/// </para>
/// <list type="bullet">
/// <item><c>income</c>, <c>debt_borrow</c>, <c>loan_collect</c>, <c>deferred</c> → balance += amount</item>
/// <item><c>direct</c>, <c>split</c>, <c>debt_repay</c>, <c>loan_give</c> → balance -= amount</item>
/// <item><c>transfer</c> outbound (source side) stored with negative amount → balance += amount</item>
/// <item><c>transfer</c> inbound (destination side) stored with positive amount → balance += amount</item>
/// </list>
/// <para>
/// Soft-deleted originals are excluded by the global query filter. <c>reversal</c> rows are
/// retained purely for audit and explicitly excluded here — the delete flow already returned the
/// balance to its pre-transaction baseline by inverting the original at delete time.
/// </para>
/// </summary>
public sealed class BalanceCalculator : IBalanceCalculator
{
    private readonly IApplicationDbContext _db;

    /// <summary>Creates the calculator.</summary>
    public BalanceCalculator(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<decimal> RecalculateAsync(Guid sourceId, CancellationToken cancellationToken = default)
    {
        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == sourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null)
            throw new NotFoundException("Source was not found.");

        var derived = await ComputeAsync(sourceId, cancellationToken).ConfigureAwait(false);

        if (source.Balance != derived)
            source.Balance = derived;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return derived;
    }

    /// <inheritdoc/>
    public Task<decimal> PreviewAsync(Guid sourceId, CancellationToken cancellationToken = default)
        => ComputeAsync(sourceId, cancellationToken);

    private async Task<decimal> ComputeAsync(Guid sourceId, CancellationToken cancellationToken)
    {
        var sourceMeta = await _db.FinSources.AsNoTracking()
            .Where(s => s.Id == sourceId)
            .Select(s => new { s.InitialBalance })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (sourceMeta is null)
            throw new NotFoundException("Source was not found.");

        var legs = await _db.FinTransactions.AsNoTracking()
            .Where(t => t.SourceId == sourceId
                        && t.Status != TxnStatus.Cancelled
                        && t.Type != TransactionType.Reversal
                        && !t.IsDeleted)
            .Select(t => new { t.Type, t.Amount })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var running = sourceMeta.InitialBalance ?? 0m;
        foreach (var leg in legs)
            running = ApplyLeg(running, leg.Type, leg.Amount);

        return decimal.Round(running, 2, MidpointRounding.ToEven);
    }

    private static decimal ApplyLeg(decimal running, TransactionType type, decimal amount) =>
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
            // Transfers store outbound amount as negative, inbound as positive — a plain += rebuilds correctly.
            TransactionType.Transfer => running + amount,
            TransactionType.BalanceAdjustment => running + amount,
            // Reversal rows are excluded above; treat any stray row as neutral.
            _ => running,
        };
}
