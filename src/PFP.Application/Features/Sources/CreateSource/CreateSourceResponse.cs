using PFP.Application.Features.Sources.Common;

namespace PFP.Application.Features.Sources.CreateSource;

/// <summary>Payload returned after a successful <see cref="CreateSourceCommand"/>.</summary>
public sealed record CreateSourceResponse(FinSourceDto Source);
