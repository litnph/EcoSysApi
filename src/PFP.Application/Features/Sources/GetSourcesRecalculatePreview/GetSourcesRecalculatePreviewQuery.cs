using MediatR;
using PFP.Application.Features.Sources.Common;

namespace PFP.Application.Features.Sources.GetSourcesRecalculatePreview;

public sealed record GetSourcesRecalculatePreviewQuery : IRequest<GetSourcesRecalculatePreviewResponse>;

public sealed record GetSourcesRecalculatePreviewResponse(
    IReadOnlyList<SourceRecalculatePreviewItemDto> Items);
