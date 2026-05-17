using MediatR;

namespace PFP.Application.Features.FeatureFlags.CreateFeatureFlag;

public sealed record CreateFeatureFlagCommand(
    string Key,
    string Name,
    string? Description,
    bool IsEnabledGlobal,
    int RolloutPercentage,
    bool IsArchived) : IRequest<CreateFeatureFlagResponse>;

/// <summary>API projection after insert.</summary>
public sealed record CreateFeatureFlagResponse(Guid Id);
