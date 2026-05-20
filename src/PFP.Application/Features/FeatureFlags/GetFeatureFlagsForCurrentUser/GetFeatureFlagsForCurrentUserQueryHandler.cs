using MediatR;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Models;

namespace PFP.Application.Features.FeatureFlags.GetFeatureFlagsForCurrentUser;

public sealed class GetFeatureFlagsForCurrentUserQueryHandler
    : IRequestHandler<GetFeatureFlagsForCurrentUserQuery, IReadOnlyList<FeatureFlagForPrincipalDto>>
{
    private readonly IFeatureFlagService _featureFlags;
    private readonly ICurrentUserService _currentUser;

    public GetFeatureFlagsForCurrentUserQueryHandler(IFeatureFlagService featureFlags, ICurrentUserService currentUser)
    {
        _featureFlags = featureFlags;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<FeatureFlagForPrincipalDto>> Handle(
        GetFeatureFlagsForCurrentUserQuery request,
        CancellationToken cancellationToken) =>
        _featureFlags.GetAllResolvedForPrincipalAsync(_currentUser.UserId, null, cancellationToken);
}
