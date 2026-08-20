using PFP.Application.Features.Transactions.CreateTransaction;

namespace PFP.Application.Features.Transactions.ImportTransactions;

/// <summary>Result for one transaction import row.</summary>
public sealed record TransactionImportRowResult(
    int Index,
    bool Success,
    Guid? TransactionId,
    string? ErrorCode,
    string? Message);

/// <summary>Non-persistent validation result for an import batch.</summary>
public sealed record PreviewTransactionImportResponse(
    bool IsValid,
    int ValidatedCount,
    IReadOnlyList<TransactionImportRowResult> Rows);

/// <summary>Persistent commit result for an import batch.</summary>
public sealed record CommitTransactionImportResponse(
    bool AllowPartial,
    int CreatedCount,
    int FailedCount,
    IReadOnlyList<TransactionImportRowResult> Rows);

internal static class TransactionImportErrors
{
    public static TransactionImportRowResult FromException(int index, Exception exception)
    {
        (string code, string message) = exception switch
        {
            FluentValidation.ValidationException validation => (
                "validation_failed",
                string.Join(" ", validation.Errors.Select(error => error.ErrorMessage).Distinct())),
            PFP.Application.Common.Exceptions.BusinessRuleException business => ("business_rule", business.Message),
            PFP.Application.Common.Exceptions.NotFoundException notFound => ("not_found", notFound.Message),
            PFP.Application.Common.Exceptions.ConcurrencyConflictException conflict => ("concurrency_conflict", conflict.Message),
            PFP.Application.Common.Exceptions.IdempotencyConflictException conflict => ("idempotency_conflict", conflict.Message),
            _ => ("import_failed", "The transaction import row could not be processed."),
        };

        return new TransactionImportRowResult(index, false, null, code, message);
    }

    public static TransactionImportRowResult Success(int index, CreateTransactionResponse response) =>
        new(index, true, response.Transaction.Id, null, null);
}
