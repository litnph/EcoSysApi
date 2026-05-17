using PFP.Application.Features.Sources.Common;

namespace PFP.Application.Features.Sources.GetSourceById;

/// <summary>Detail payload for one finance source.</summary>
public sealed record GetSourceByIdResponse(FinSourceDto Source);
