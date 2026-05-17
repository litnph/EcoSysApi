using MediatR;
using PFP.Application.Common.Models;

namespace PFP.Application.Features.FeatureFlags.GetFeatureFlagsForCurrentUser;

/// <summary>Loads every persisted feature flag with resolved enabled state.</summary>
public sealed record GetFeatureFlagsForCurrentUserQuery : IRequest<IReadOnlyList<FeatureFlagForPrincipalDto>>;
