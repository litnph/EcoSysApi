using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.UpdateCategory;

/// <summary>Updates an existing finance category.</summary>
public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    CategoryKind Kind,
    Guid? ParentId,
    string? Icon,
    string? Color,
    int? SortOrder,
    bool IsDefault,
    CategoryNecessityLevel? NecessityLevel = null) : IRequest<UpdateCategoryResponse>;
