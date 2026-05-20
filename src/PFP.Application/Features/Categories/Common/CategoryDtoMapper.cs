using PFP.Domain.Entities;

namespace PFP.Application.Features.Categories.Common;

/// <summary>Maps <see cref="FinCategory"/> rows to API DTOs.</summary>
public static class CategoryDtoMapper
{
    /// <summary>Maps one entity to a tree DTO without loading children (post-mutation responses).</summary>
    public static CategoryTreeNodeDto ToSingleNode(FinCategory c) =>
        new(
            c.Id,
            c.Name,
            c.Kind,
            c.ParentId,
            c.Icon,
            c.Color,
            c.SortOrder,
            c.IsDefault,
            Array.Empty<CategoryTreeNodeDto>());

    /// <summary>Maps a single entity to a flat DTO.</summary>
    public static CategoryFlatDto ToFlatDto(FinCategory c) =>
        new(
            c.Id,
            c.Name,
            c.Kind,
            c.ParentId,
            c.Icon,
            c.Color,
            c.SortOrder,
            c.IsDefault,
            c.Depth);

    /// <summary>Builds a tree from a flat materialised list (all same module + kind).</summary>
    public static IReadOnlyList<CategoryTreeNodeDto> BuildTree(IReadOnlyList<FinCategory> flat)
    {
        var byId = flat.ToDictionary(x => x.Id);
        var childrenByParent = flat
            .Where(x => x.ParentId is not null)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList());

        CategoryTreeNodeDto BuildNode(FinCategory c)
        {
            var childList = childrenByParent.TryGetValue(c.Id, out var list)
                ? list.Select(BuildNode).ToList()
                : new List<CategoryTreeNodeDto>();
            return new CategoryTreeNodeDto(
                c.Id,
                c.Name,
                c.Kind,
                c.ParentId,
                c.Icon,
                c.Color,
                c.SortOrder,
                c.IsDefault,
                childList);
        }

        var roots = flat
            .Where(c => c.ParentId is null || !byId.ContainsKey(c.ParentId.Value))
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToList();

        return roots.Select(BuildNode).ToList();
    }
}
