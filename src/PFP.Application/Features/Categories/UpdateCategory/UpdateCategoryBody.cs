using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.UpdateCategory;

/// <summary>JSON body for <c>PUT /finance/categories/:id</c> (route id excluded).</summary>
public sealed record UpdateCategoryBody(
    string Name,
    CategoryKind Kind,
    Guid? ParentId,
    string? Icon,
    string? Color,
    int? SortOrder,
    bool IsDefault,
    CategoryNecessityLevel? NecessityLevel = null);
