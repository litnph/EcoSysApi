using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.DeleteTransaction;

/// <summary>
/// Soft-deletes a transaction, emits reversal row(s), reverts balances, appends deleted history with reason,
/// and records a manual audit row for the original transaction id.
/// </summary>
public sealed class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, DeleteTransactionResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public DeleteTransactionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc cref="IRequestHandler{DeleteTransactionCommand, DeleteTransactionResponse}.Handle" />
    public async Task<DeleteTransactionResponse> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var orig = await _db.FinTransactions
            .Include(t => t.Source)
            .Include(t => t.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            .ConfigureAwait(false);

        if (orig is null)
            throw new NotFoundException("Transaction was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(orig.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to delete this transaction.");

        if (_currentUser.CurrentOrgId is { } orgId && orig.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this transaction.");

        FinTransaction? partner = null;
        if (orig.Type == TransactionType.Transfer)
        {
            if (orig.RefTxnId is null)
                throw new BusinessRuleException("Transfer transaction is missing a counterpart link.");

            partner = await _db.FinTransactions
                .Include(t => t.Source)
                .FirstOrDefaultAsync(
                    t => t.Id == orig.RefTxnId.Value && t.SmoduleId == orig.SmoduleId,
                    cancellationToken)
                .ConfigureAwait(false);

            if (partner is null || partner.RefTxnId != orig.Id)
                throw new BusinessRuleException("The linked transfer counterpart is missing or inconsistent.");

            if (partner.IsDeleted)
                throw new BusinessRuleException("The linked transfer counterpart is already deleted.");
        }

        var beforeAuditJson = TransactionHistoryJson.BuildTransactionStateSnapshot(orig);
        var utcNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(utcNow);
        var userId = _currentUser.UserId.Value;

        // Spec §4.2: the soft-delete, reversal row, history row, balance revert, and audit row MUST
        // commit atomically. Wrap with CreateExecutionStrategy so the explicit transaction composes
        // with the SQL Server retrying execution strategy configured by EnableRetryOnFailure.
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var dbTx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            orig.IsDeleted = true;
            orig.DeletedAt = utcNow;
            orig.DeletedBy = userId;
            orig.Version += 1;
            orig.UpdatedBy = userId;
            orig.LastSessionId = _currentUser.SessionId;

            _db.FinTransactionHistory.Add(new FinTransactionHistory
            {
                TransactionId = orig.Id,
                Version = orig.Version,
                ChangedBy = userId,
                SessionId = _currentUser.SessionId,
                ChangeType = HistoryChangeType.Deleted,
                ChangedFields = null,
                Snapshot = TransactionHistoryJson.BuildTransactionStateSnapshot(orig),
                ChangeReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
            });

            if (partner is not null)
            {
                partner.IsDeleted = true;
                partner.DeletedAt = utcNow;
                partner.DeletedBy = userId;
            }

            if (orig.Type == TransactionType.Transfer && partner is not null)
            {
                _db.FinTransactions.Add(CreateReversalForLeg(orig, today));
                _db.FinTransactions.Add(CreateReversalForLeg(partner, today));
                ApplyBalanceRevert(orig.Source, orig);
                ApplyBalanceRevert(partner.Source, partner);
            }
            else
            {
                _db.FinTransactions.Add(CreateReversalForLeg(orig, today));
                ApplyBalanceRevert(orig.Source, orig);
            }

            var auditNow = DateTime.UtcNow;
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                SessionId = _currentUser.SessionId,
                EntityType = "fin_transactions",
                EntityId = orig.Id,
                Action = AuditAction.Deleted,
                BeforeSnapshot = beforeAuditJson,
                AfterSnapshot = null,
                ChangedFields = null,
                IpAddress = _currentUser.IpAddress,
                UserAgent = _currentUser.UserAgent,
                CreatedAt = auditNow,
                UpdatedAt = auditNow,
            });

            if (orig.Type == TransactionType.Deferred && orig.BillingCycleId is { } billCycleId)
            {
                var bc = await _db.FinBillingCycles
                    .FirstOrDefaultAsync(c => c.Id == billCycleId, cancellationToken)
                    .ConfigureAwait(false);
                if (bc is not null && bc.Status == BillingCycleStatus.Open)
                    bc.TotalAmount -= orig.Amount;
            }

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await dbTx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);

        return new DeleteTransactionResponse(orig.Id);
    }

    private static FinTransaction CreateReversalForLeg(FinTransaction sourceTxn, DateOnly txnDate)
    {
        var note = BuildReversalNote(sourceTxn.Note);
        var description = $"Reversal of transaction {sourceTxn.Id}";
        if (description.Length > 512)
            description = description[..512];

        return new FinTransaction
        {
            SmoduleId = sourceTxn.SmoduleId,
            Type = TransactionType.Reversal,
            Status = TxnStatus.Completed,
            Amount = sourceTxn.Amount,
            Currency = sourceTxn.Currency,
            TxnDate = txnDate,
            SourceId = sourceTxn.SourceId,
            DestSourceId = sourceTxn.DestSourceId,
            CategoryId = sourceTxn.CategoryId,
            RefTxnId = sourceTxn.Id,
            BillingCycleId = null,
            MonthlyPeriodId = null,
            Description = description,
            Note = note,
        };
    }

    private static string BuildReversalNote(string? originalNote)
    {
        const string prefix = "Hoàn tác: ";
        var body = originalNote?.Trim();
        var combined = string.IsNullOrEmpty(body) ? prefix.TrimEnd() : prefix + body;
        return combined.Length <= 500 ? combined : combined[..500];
    }

    private static void ApplyBalanceRevert(FinSource source, FinTransaction txn)
    {
        switch (txn.Type)
        {
            case TransactionType.Income:
                source.Balance -= txn.Amount;
                break;
            case TransactionType.Direct:
                source.Balance += txn.Amount;
                break;
            case TransactionType.Split:
                source.Balance += txn.Amount;
                break;
            case TransactionType.Deferred:
                source.Balance -= txn.Amount;
                break;
            case TransactionType.Transfer:
                source.Balance -= txn.Amount;
                break;
            default:
                throw new InvalidOperationException($"Unexpected transaction type for balance revert: {txn.Type}.");
        }
    }
}
