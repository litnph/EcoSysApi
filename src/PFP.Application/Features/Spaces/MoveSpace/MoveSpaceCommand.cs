using MediatR;

namespace PFP.Application.Features.Spaces.MoveSpace;

/// <summary>Relocates a space under a different parent — full subtree paths and depths are rewritten (spec §4.5).</summary>
public sealed record MoveSpaceCommand(Guid SpaceId, Guid? NewParentId) : IRequest<MoveSpaceResponse>;
