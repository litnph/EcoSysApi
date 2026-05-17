using PFP.Application.Features.Categories.Common;

namespace PFP.Application.Features.Categories.UpdateCategory;

/// <summary>Payload returned after <see cref="UpdateCategoryCommand"/>.</summary>
public sealed record UpdateCategoryResponse(CategoryTreeNodeDto Category);
