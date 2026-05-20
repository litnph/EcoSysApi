namespace PFP.Application.Features.Tags.Common;

/// <summary>Finance tag catalogue row.</summary>
public sealed record TagListItemDto(Guid Id, string Name, string Color, int UsageCount);

public sealed record CreateTagResponse(Guid Id);

public sealed record UpdateTagResponse(Guid Id);

public sealed record TaggedEntityRefDto(string ModuleCode, string EntityType, Guid EntityId);

public sealed record GetEntitiesByTagResponse(IReadOnlyList<TaggedEntityRefDto> Entities);
