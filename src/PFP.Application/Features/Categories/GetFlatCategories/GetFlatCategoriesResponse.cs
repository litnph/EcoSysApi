using PFP.Application.Features.Categories.Common;

namespace PFP.Application.Features.Categories.GetFlatCategories;

/// <summary>Flat rows returned by <see cref="GetFlatCategoriesQuery"/>.</summary>
public sealed record GetFlatCategoriesResponse(IReadOnlyList<CategoryFlatDto> Items);
