using MediatR;

namespace PFP.Application.Features.FeatureFlags.UpdateFeatureFlag;

public sealed record UpdateFeatureFlagCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsEnabledGlobal,
    int RolloutPercentage,
    bool IsArchived) : IRequest<UpdateFeatureFlagResponse>;

public sealed record UpdateFeatureFlagResponse(Guid Id);
