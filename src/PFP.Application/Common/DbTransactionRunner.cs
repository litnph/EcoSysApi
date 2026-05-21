using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Common;

/// <summary>
/// Runs work inside an explicit database transaction compatible with
/// <c>SqlServerRetryingExecutionStrategy</c> (<c>EnableRetryOnFailure</c>).
/// </summary>
public static class DbTransactionRunner
{
    /// <summary>Executes <paramref name="action"/> inside a retriable transaction and commits on success.</summary>
    public static Task ExecuteAsync(
        IApplicationDbContext db,
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await action(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        });
    }

    /// <summary>Executes <paramref name="action"/> inside a retriable transaction and returns its result.</summary>
    public static Task<T> ExecuteAsync<T>(
        IApplicationDbContext db,
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var result = await action(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        });
    }
}
