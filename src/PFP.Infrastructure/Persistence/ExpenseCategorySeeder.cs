using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// Seed / thay thế cây danh mục chi tiêu chuẩn.
/// Bật <c>Seed:ExpenseCategories</c>; dùng <c>Seed:ExpenseCategories:Reset</c> để xóa danh mục chi tiêu cũ trước khi seed lại.
/// </summary>
public static class ExpenseCategorySeeder
{
    public static async Task EnsureAsync(
        AppDbContext db,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue("Seed:ExpenseCategories", false))
            return;

        var reset = configuration.GetValue("Seed:ExpenseCategories:Reset", false);
        var alreadySeeded = await db.FinCategories
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Code == ExpenseCategorySeedCatalog.MarkerCode, cancellationToken)
            .ConfigureAwait(false);

        if (reset)
        {
            await ReplaceAsync(db, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!alreadySeeded)
        {
            await ReplaceAsync(db, cancellationToken).ConfigureAwait(false);
            return;
        }

        await ApplyVisualsAsync(db, cancellationToken).ConfigureAwait(false);
    }

    public static async Task ReplaceAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await db.FinCategories
            .IgnoreQueryFilters()
            .Where(c => c.Kind == CategoryKind.Expense)
            .ExecuteUpdateAsync(
                s => s.SetProperty(c => c.ParentId, (Guid?)null),
                cancellationToken)
            .ConfigureAwait(false);

        await db.FinCategories
            .IgnoreQueryFilters()
            .Where(c => c.Kind == CategoryKind.Expense)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        var sortOrder = 0;
        foreach (var root in ExpenseCategorySeedCatalog.Roots)
        {
            sortOrder++;
            var rootEntity = CreateCategory(root, parentId: null, depth: 0, sortOrder, isDefault: sortOrder == 1);
            db.FinCategories.Add(rootEntity);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var childSort = 0;
            foreach (var child in root.Children)
            {
                childSort++;
                db.FinCategories.Add(CreateCategory(child, rootEntity.Id, depth: 1, childSort, isDefault: false));
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task ApplyVisualsAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        foreach (var node in ExpenseCategorySeedCatalog.Flatten())
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

    private static FinCategory CreateCategory(
        ExpenseCategorySeedNode node,
        Guid? parentId,
        int depth,
        int sortOrder,
        bool isDefault) =>
        new()
        {
            Name = node.Name,
            Code = node.Code,
            Kind = CategoryKind.Expense,
            ParentId = parentId,
            Depth = depth,
            SortOrder = sortOrder,
            IsDefault = isDefault,
            IsSystem = true,
            Icon = node.Icon,
            Color = node.Color,
            NecessityLevel = node.NecessityLevel,
        };
}
