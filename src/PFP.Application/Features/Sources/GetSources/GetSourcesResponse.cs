using PFP.Application.Features.Sources.Common;

namespace PFP.Application.Features.Sources.GetSources;

/// <summary>Ordered collection returned by <see cref="GetSourcesQuery"/>.</summary>
public sealed record GetSourcesResponse(IReadOnlyList<FinSourceDto> Sources);
