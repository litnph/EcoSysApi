using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.TransactionClassificationRules;
using PFP.Application.Features.Transactions.CreateTransaction;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.ImportTransactions;

public interface ITransactionImportClassifier
{
    Task<CreateTransactionCommand> ApplyAsync(CreateTransactionCommand command, CancellationToken cancellationToken);
}

public sealed class TransactionImportClassifier : ITransactionImportClassifier
{
    private readonly ICurrentUserService _currentUser;
    private readonly ITransactionClassificationService _classification;
    public TransactionImportClassifier(ICurrentUserService currentUser, ITransactionClassificationService classification)
    {
        _currentUser = currentUser;
        _classification = classification;
    }

    public async Task<CreateTransactionCommand> ApplyAsync(CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId) throw new UnauthorizedAppException("Authentication is required.");
        if (command.Type is not (TransactionType.Direct or TransactionType.Deferred or TransactionType.Split)) return command;
        if (command.CategoryId is not null && command.TagIds is not null) return command;
        var match = await _classification.MatchAsync(userId, command.Description, cancellationToken);
        if (match is null) return command;
        return command with
        {
            CategoryId = command.CategoryId ?? match.CategoryId,
            TagIds = command.TagIds ?? (match.TagId is { } tagId ? [tagId] : []),
        };
    }
}
