using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.TransactionClassificationRules;

public sealed record TransactionClassificationMatch(Guid RuleId, Guid CategoryId, Guid? TagId);

public interface ITransactionClassificationService
{
    Task<TransactionClassificationMatch?> MatchAsync(Guid userId, string? content, CancellationToken cancellationToken);
}

public sealed class TransactionClassificationService : ITransactionClassificationService
{
    private readonly IApplicationDbContext _db;
    private readonly Dictionary<Guid, Task<List<TransactionClassificationRule>>> _rulesByUser = new();
    public TransactionClassificationService(IApplicationDbContext db) => _db = db;

    public async Task<TransactionClassificationMatch?> MatchAsync(
        Guid userId,
        string? content,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        if (!_rulesByUser.TryGetValue(userId, out var rulesTask))
        {
            rulesTask = _db.TransactionClassificationRules
                .AsNoTracking()
                .Where(rule => rule.UserId == userId
                               && rule.IsActive
                               && !rule.Category.IsDeleted
                               && (rule.TagId == null || !rule.Tag!.IsDeleted))
                .ToListAsync(cancellationToken);
            _rulesByUser[userId] = rulesTask;
        }
        var rules = await rulesTask.ConfigureAwait(false);
        var best = TransactionContentMatcher.SelectBest(rules, content);
        return best is null ? null : new(best.Id, best.CategoryId, best.TagId);
    }
}
