using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Transactions.Common;

public interface ITransactionTagWriter
{
    Task AddAsync(Guid transactionId, IReadOnlyList<Guid>? tagIds, Guid userId, CancellationToken cancellationToken);
}

public sealed class TransactionTagWriter : ITransactionTagWriter
{
    private readonly IApplicationDbContext _db;
    public TransactionTagWriter(IApplicationDbContext db) => _db = db;

    public async Task AddAsync(Guid transactionId, IReadOnlyList<Guid>? tagIds, Guid userId, CancellationToken cancellationToken)
    {
        var ids = tagIds?.Where(id => id != Guid.Empty).Distinct().ToArray() ?? [];
        if (ids.Length == 0) return;
        if (ids.Length > 20) throw new BusinessRuleException("A transaction can have at most 20 tags.");
        var tags = await _db.Tags.Where(tag => ids.Contains(tag.Id)).ToListAsync(cancellationToken);
        if (tags.Count != ids.Length) throw new BusinessRuleException("One or more selected tags are not available.");
        var existing = await _db.EntityTags
            .Where(link => link.EntityType == nameof(FinTransaction) && link.EntityId == transactionId && ids.Contains(link.TagId))
            .Select(link => link.TagId).ToListAsync(cancellationToken);
        foreach (var tag in tags.Where(tag => !existing.Contains(tag.Id)))
        {
            _db.EntityTags.Add(new EntityTag
            {
                TagId = tag.Id, EntityType = nameof(FinTransaction), EntityId = transactionId, TaggedBy = userId,
            });
            tag.UsageCount++;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }
}
