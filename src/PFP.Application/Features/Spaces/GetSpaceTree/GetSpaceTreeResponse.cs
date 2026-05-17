using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.GetSpaceTree;

/// <summary>Envelope for the rooted space forest (usually exactly one seeded root).</summary>
public sealed record GetSpaceTreeResponse(IReadOnlyList<SpaceTreeDto> Roots);

/// <summary>One tree node describing a single <c>SPACE</c> row.</summary>
public sealed record SpaceTreeDto(
    Guid Id,
    Guid OrgId,
    string Name,
    SpaceType Type,
    int Depth,
    string Path,
    bool FinanceModuleEnabled,
    int SortOrder,
    Guid? ParentId,
    IReadOnlyList<SpaceTreeDto> Children);
