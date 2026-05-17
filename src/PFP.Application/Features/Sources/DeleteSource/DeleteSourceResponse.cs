namespace PFP.Application.Features.Sources.DeleteSource;

/// <summary>Payload returned after a successful <see cref="DeleteSourceCommand"/>.</summary>
public sealed record DeleteSourceResponse(Guid Id);
