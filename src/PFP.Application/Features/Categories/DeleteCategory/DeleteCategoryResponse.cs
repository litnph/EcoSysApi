namespace PFP.Application.Features.Categories.DeleteCategory;

/// <summary>Payload returned after <see cref="DeleteCategoryCommand"/>.</summary>
public sealed record DeleteCategoryResponse(Guid Id);
