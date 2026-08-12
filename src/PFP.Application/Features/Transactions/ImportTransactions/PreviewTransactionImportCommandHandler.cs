using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Transactions.ImportTransactions;

public sealed class PreviewTransactionImportCommandHandler
    : IRequestHandler<PreviewTransactionImportCommand, PreviewTransactionImportResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;

    public PreviewTransactionImportCommandHandler(IApplicationDbContext db, ISender sender)
    {
        _db = db;
        _sender = sender;
    }

    public async Task<PreviewTransactionImportResponse> Handle(
        PreviewTransactionImportCommand request,
        CancellationToken cancellationToken)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
            var rows = new List<TransactionImportRowResult>(request.Items.Count);

            try
            {
                for (var index = 0; index < request.Items.Count; index++)
                {
                    var response = await _sender.Send(request.Items[index], cancellationToken)
                        .ConfigureAwait(false);
                    rows.Add(TransactionImportErrors.Success(index, response));
                }

                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return new PreviewTransactionImportResponse(true, rows.Count, rows);
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                rows.Add(TransactionImportErrors.FromException(rows.Count, exception));
                return new PreviewTransactionImportResponse(false, rows.Count - 1, rows);
            }
        }).ConfigureAwait(false);
    }
}
