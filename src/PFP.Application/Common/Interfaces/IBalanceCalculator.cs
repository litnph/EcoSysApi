namespace PFP.Application.Common.Interfaces;

/// <summary>
/// Reconciliation service for <c>FIN_SOURCES.balance</c>. Implements the "balance is never
/// updated directly" invariant from spec §4.6 — the only legitimate writes to the column are
/// (a) creation of a <c>FIN_TRANSACTION</c>, (b) a reversal row from the soft-delete flow, and
/// (c) a deterministic recompute through <see cref="RecalculateAsync"/>.
/// <para>
/// Used by:
/// </para>
/// <list type="bullet">
/// <item>Admin "recalculate balance" endpoint when corruption is suspected.</item>
/// <item>Nightly reconciliation job (future) that scans every source and re-derives the balance
/// from the transaction history.</item>
/// </list>
/// </summary>
public interface IBalanceCalculator
{
    /// <summary>
    /// Recomputes the canonical balance for <paramref name="sourceId"/> from the
    /// <c>FIN_TRANSACTIONS</c> history (excluding soft-deleted rows) plus the source's
    /// <c>InitialBalance</c>, persists the result if it differs from the stored value, and
    /// returns the recomputed amount.
    /// </summary>
    /// <param name="sourceId">Identifier of the <c>FIN_SOURCES</c> row.</param>
    /// <param name="cancellationToken">Standard cancellation token.</param>
    /// <returns>The post-reconciliation balance value.</returns>
    Task<decimal> RecalculateAsync(Guid sourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the balance that <em>would</em> result if reconciliation ran right now,
    /// without persisting the value. Useful for drift detection and audit reports.
    /// </summary>
    /// <param name="sourceId">Identifier of the <c>FIN_SOURCES</c> row.</param>
    /// <param name="cancellationToken">Standard cancellation token.</param>
    Task<decimal> PreviewAsync(Guid sourceId, CancellationToken cancellationToken = default);
}
