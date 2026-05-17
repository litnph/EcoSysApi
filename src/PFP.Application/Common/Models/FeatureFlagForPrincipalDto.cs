namespace PFP.Application.Common.Models;

/// <summary>One platform feature flag resolved for the current principal (JWT user + optional org).</summary>
public sealed record FeatureFlagForPrincipalDto(
    Guid Id,
    string Key,
    string Name,
    string? Description,
    bool IsEnabledGlobal,
    int RolloutPercentage,
    bool IsArchived,
    bool IsEnabledForCurrentPrincipal);
