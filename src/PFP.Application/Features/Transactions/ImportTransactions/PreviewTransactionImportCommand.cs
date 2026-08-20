using MediatR;
using PFP.Application.Features.Transactions.CreateTransaction;

namespace PFP.Application.Features.Transactions.ImportTransactions;

/// <summary>Runs the real transaction pipeline in a database transaction that is always rolled back.</summary>
public sealed record PreviewTransactionImportCommand(
    IReadOnlyList<CreateTransactionCommand> Items) : IRequest<PreviewTransactionImportResponse>;
