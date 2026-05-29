using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence;

/// <summary>Seed danh mục thu nhập chuẩn. Bật qua <c>Seed:IncomeCategories</c>.</summary>
public static class IncomeCategorySeeder
{
    public static async Task EnsureAsync(
        AppDbContext db,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue("Seed:IncomeCategories", false))
            return;

        var reset = configuration.GetValue("Seed:IncomeCategories:Reset", false);
        var alreadySeeded = await db.FinCategories
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Code == IncomeCategorySeedCatalog.MarkerCode, cancellationToken)
            .ConfigureAwait(false);

        if (reset)
        {
            await ReplaceAsync(db, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!alreadySeeded)
            await ReplaceAsync(db, cancellationToken).ConfigureAwait(false);
        else
            await ApplyVisualsAsync(db, cancellationToken).ConfigureAwait(false);
    }

    public static async Task ReplaceAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await db.FinCategories
            .IgnoreQueryFilters()
            .Where(c => c.Kind == CategoryKind.Income)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        var sortOrder = 0;
        foreach (var node in IncomeCategorySeedCatalog.Items)
        {
            sortOrder++;
            db.FinCategories.Add(new FinCategory
            {
                Name = node.Name,
                Code = node.Code,
                Kind = CategoryKind.Income,
                ParentId = null,
                Depth = 0,
                SortOrder = sortOrder,
                IsDefault = sortOrder == 1,
                IsSystem = true,
                Icon = node.Icon,
                Color = node.Color,
            });
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task ApplyVisualsAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        foreach (var node in IncomeCategorySeedCatalog.Items)
        {
            await db.FinCategories
                .IgnoreQueryFilters()
                .Where(c => c.Code == node.Code)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(c => c.Icon, node.Icon)
                        .SetProperty(c => c.Color, node.Color),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
