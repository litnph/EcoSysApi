using MediatR;

namespace PFP.Application.Features.FeatureFlags.DeleteFeatureFlagOverride;

public sealed record DeleteFeatureFlagOverrideCommand(Guid FlagId, Guid OverrideId) : IRequest<Unit>;
