using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.CreateSpace;

/// <summary>Creates an organisation space and applies membership inheritance rules (spec §4.5).</summary>
public sealed record CreateSpaceCommand(
    Guid OrgId,
    string Name,
    SpaceType Type,
    Guid? ParentId,
    string? Description,
    int? SortOrder = null) : IRequest<CreateSpaceResponse>;
