using PFP.Application.Features.Categories.Common;

namespace PFP.Application.Features.Categories.CreateCategory;

/// <summary>Payload returned after <see cref="CreateCategoryCommand"/>.</summary>
public sealed record CreateCategoryResponse(CategoryTreeNodeDto Category);
