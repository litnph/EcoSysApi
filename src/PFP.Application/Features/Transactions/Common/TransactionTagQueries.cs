using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Loads tags attached to finance transactions.</summary>
public static class TransactionTagQueries
{
    public static async Task<IReadOnlyList<TransactionTagDto>> ForTransactionAsync(
        IApplicationDbContext db,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        return await (
            from link in db.EntityTags.AsNoTracking()
            join tag in db.Tags.AsNoTracking() on link.TagId equals tag.Id
            where link.EntityType == nameof(FinTransaction)
                  && link.EntityId == transactionId
            orderby tag.Name
            select new TransactionTagDto(tag.Id, tag.Name, tag.Color)
        ).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<TransactionTagDto>>> ForTransactionsMapAsync(
        IApplicationDbContext db,
        IEnumerable<Guid> transactionIds,
        CancellationToken cancellationToken)
    {
        var ids = transactionIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, IReadOnlyList<TransactionTagDto>>();

        var rows = await (
            from link in db.EntityTags.AsNoTracking()
            join tag in db.Tags.AsNoTracking() on link.TagId equals tag.Id
            where link.EntityType == nameof(FinTransaction)
                  && ids.Contains(link.EntityId)
            orderby tag.Name
            select new { link.EntityId, Tag = new TransactionTagDto(tag.Id, tag.Name, tag.Color) }
        ).ToListAsync(cancellationToken).ConfigureAwait(false);

        return rows
            .GroupBy(r => r.EntityId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<TransactionTagDto>)g.Select(x => x.Tag).ToList());
    }
}
