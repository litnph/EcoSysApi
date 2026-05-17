using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.FeatureFlags.CreateFeatureFlagOverride;

public sealed class CreateFeatureFlagOverrideCommandHandler
    : IRequestHandler<CreateFeatureFlagOverrideCommand, CreateFeatureFlagOverrideResponse>
{
    private readonly IApplicationDbContext _db;

    public CreateFeatureFlagOverrideCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<CreateFeatureFlagOverrideResponse> Handle(
        CreateFeatureFlagOverrideCommand request,
        CancellationToken cancellationToken)
    {
        var flagExists = await _db.FeatureFlags.AnyAsync(f => f.Id == request.FlagId, cancellationToken).ConfigureAwait(false);
        if (!flagExists)
            throw new NotFoundException("Feature flag not found.");

        var entity = new FeatureFlagOverride
        {
            FlagId = request.FlagId,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            IsEnabled = request.IsEnabled,
            ExpiresAt = request.ExpiresAt,
        };

        _db.FeatureFlagOverrides.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateFeatureFlagOverrideResponse(entity.Id);
    }
}
