namespace PFP.Application.Features.Spaces.MoveSpace;

/// <summary>Payload returned after a successful subtree move.</summary>
public sealed record MoveSpaceResponse(
    Guid Id,
    Guid OrgId,
    Guid? ParentId,
    string Path,
    int Depth);
