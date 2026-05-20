using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.GetFlatCategories;

/// <summary>Returns a flat category list for transaction forms.</summary>
public sealed record GetFlatCategoriesQuery(CategoryKind Kind) : IRequest<GetFlatCategoriesResponse>;
