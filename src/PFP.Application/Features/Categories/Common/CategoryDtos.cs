using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.Common;

/// <summary>Nested category node for tree APIs.</summary>
public sealed record CategoryTreeNodeDto(
    Guid Id,
    string Name,
    CategoryKind Kind,
    Guid? ParentId,
    string? Icon,
    string? Color,
    int SortOrder,
    bool IsDefault,
    IReadOnlyList<CategoryTreeNodeDto> Children);

/// <summary>Flat category row for dropdowns.</summary>
public sealed record CategoryFlatDto(
    Guid Id,
    string Name,
    CategoryKind Kind,
    Guid? ParentId,
    string? Icon,
    string? Color,
    int SortOrder,
    bool IsDefault,
    int Depth);
