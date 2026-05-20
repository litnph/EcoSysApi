using MediatR;
using PFP.Application.Features.Tags.Common;

namespace PFP.Application.Features.Tags.GetTags;

public sealed record GetTagsQuery() : IRequest<IReadOnlyList<TagListItemDto>>;
