using PFP.Application.Features.Sources.Common;

namespace PFP.Application.Features.Sources.UpdateSource;

/// <summary>Payload returned after a successful <see cref="UpdateSourceCommand"/>.</summary>
public sealed record UpdateSourceResponse(FinSourceDto Source);
