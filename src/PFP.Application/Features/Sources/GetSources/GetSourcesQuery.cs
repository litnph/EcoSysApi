using MediatR;

namespace PFP.Application.Features.Sources.GetSources;

/// <summary>Lists finance sources for a single finance space-module, ordered for UI display.</summary>
public sealed record GetSourcesQuery(Guid SmoduleId) : IRequest<GetSourcesResponse>;
