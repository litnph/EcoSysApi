using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Common.Utils;

/// <summary>Appends a <c>FIN_TRANSACTIONS_HISTORY</c> v1 row for a newly created transaction.</summary>
internal static class FinTransactionHistoryHelper
{
    /// <summary>Adds a <see cref="HistoryChangeType.Created"/> history row for <paramref name="txn"/>.</summary>
    public static void AddCreated(IApplicationDbContext db, ICurrentUserService currentUser, FinTransaction txn)
    {
        var now = DateTime.UtcNow;
        db.FinTransactionHistory.Add(
            new FinTransactionHistory
            {
                TransactionId = txn.Id,
                Version = 1,
                ChangedBy = currentUser.UserId,
                SessionId = currentUser.SessionId,
                ChangeType = HistoryChangeType.Created,
                ChangedFields = null,
                Snapshot = TransactionHistoryJson.BuildCreatedSnapshot(txn),
                ChangeReason = null,
                CreatedAt = now,
                UpdatedAt = now,
            });
    }
}
