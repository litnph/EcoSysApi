using MediatR;
using PFP.Application.Features.Transactions.CreateTransaction;

namespace PFP.Application.Features.Transactions.ImportTransactions;

/// <summary>Commits an import atomically by default, or explicitly row-by-row when partial mode is enabled.</summary>
public sealed record CommitTransactionImportCommand(
    IReadOnlyList<CreateTransactionCommand> Items,
    bool AllowPartial = false) : IRequest<CommitTransactionImportResponse>;
