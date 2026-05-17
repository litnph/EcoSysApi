using PFP.Application.Features.Categories.Common;

namespace PFP.Application.Features.Categories.GetCategories;

/// <summary>Tree roots returned by <see cref="GetCategoriesQuery"/>.</summary>
public sealed record GetCategoriesResponse(IReadOnlyList<CategoryTreeNodeDto> Roots);
