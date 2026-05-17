namespace PFP.Application.Features.Spaces.CreateSpace;

/// <summary>Payload returned after a successful space create.</summary>
public sealed record CreateSpaceResponse(
    Guid Id,
    Guid OrgId,
    Guid? ParentId,
    string Path,
    int Depth);
