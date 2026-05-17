using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.FeatureFlags.CreateFeatureFlagOverride;

public sealed record CreateFeatureFlagOverrideCommand(
    Guid FlagId,
    OverrideTargetType TargetType,
    Guid TargetId,
    bool IsEnabled,
    DateTime? ExpiresAt) : IRequest<CreateFeatureFlagOverrideResponse>;

public sealed record CreateFeatureFlagOverrideResponse(Guid Id);
