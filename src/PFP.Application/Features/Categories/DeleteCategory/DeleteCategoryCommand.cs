using MediatR;

namespace PFP.Application.Features.Categories.DeleteCategory;

/// <summary>Soft-deletes a finance category when it has no transactions or active children.</summary>
public sealed record DeleteCategoryCommand(Guid Id) : IRequest<DeleteCategoryResponse>;
