using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.FeatureFlags.UpdateFeatureFlag;

public sealed class UpdateFeatureFlagCommandHandler : IRequestHandler<UpdateFeatureFlagCommand, UpdateFeatureFlagResponse>
{
    private readonly IApplicationDbContext _db;

    public UpdateFeatureFlagCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<UpdateFeatureFlagResponse> Handle(UpdateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.FeatureFlags
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Feature flag not found.");

        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.IsEnabledGlobal = request.IsEnabledGlobal;
        entity.RolloutPercentage = request.RolloutPercentage;
        entity.IsArchived = request.IsArchived;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new UpdateFeatureFlagResponse(entity.Id);
    }
}
