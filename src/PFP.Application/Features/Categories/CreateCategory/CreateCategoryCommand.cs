using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.CreateCategory;

/// <summary>Creates a finance category under a space-module (tree-aware).</summary>
public sealed record CreateCategoryCommand(
    string Name,
    CategoryKind Kind,
    Guid? ParentId,
    string? Icon,
    string? Color,
    int? SortOrder,
    bool IsDefault = false) : IRequest<CreateCategoryResponse>;
