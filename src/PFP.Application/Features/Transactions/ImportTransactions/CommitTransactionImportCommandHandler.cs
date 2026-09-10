using MediatR;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Transactions.ImportTransactions;

public sealed class CommitTransactionImportCommandHandler
    : IRequestHandler<CommitTransactionImportCommand, CommitTransactionImportResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _sender;
    private readonly ITransactionImportClassifier _classifier;

    public CommitTransactionImportCommandHandler(IApplicationDbContext db, ISender sender, ITransactionImportClassifier classifier)
    {
        _db = db;
        _sender = sender;
        _classifier = classifier;
    }

    public async Task<CommitTransactionImportResponse> Handle(
        CommitTransactionImportCommand request,
        CancellationToken cancellationToken)
    {
        var rows = new List<TransactionImportRowResult>(request.Items.Count);

        if (!request.AllowPartial)
        {
            await DbTransactionRunner.ExecuteAsync(_db, async ct =>
            {
                for (var index = 0; index < request.Items.Count; index++)
                {
                    var item = await _classifier.ApplyAsync(request.Items[index], ct).ConfigureAwait(false);
                    var response = await _sender.Send(item, ct).ConfigureAwait(false);
                    rows.Add(TransactionImportErrors.Success(index, response));
                }
            }, cancellationToken).ConfigureAwait(false);

            return new CommitTransactionImportResponse(false, rows.Count, 0, rows);
        }

        for (var index = 0; index < request.Items.Count; index++)
        {
            try
            {
                var item = await _classifier.ApplyAsync(request.Items[index], cancellationToken).ConfigureAwait(false);
                var response = await _sender.Send(item, cancellationToken)
                    .ConfigureAwait(false);
                rows.Add(TransactionImportErrors.Success(index, response));
            }
            catch (Exception exception)
            {
                rows.Add(TransactionImportErrors.FromException(index, exception));
            }
        }

        var created = rows.Count(row => row.Success);
        return new CommitTransactionImportResponse(true, created, rows.Count - created, rows);
    }
}
