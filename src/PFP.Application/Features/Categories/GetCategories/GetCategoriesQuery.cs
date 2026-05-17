using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.GetCategories;

/// <summary>Returns a nested category tree for a finance module and kind.</summary>
public sealed record GetCategoriesQuery(Guid SmoduleId, CategoryKind Kind) : IRequest<GetCategoriesResponse>;
