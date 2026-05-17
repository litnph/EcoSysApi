using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.FeatureFlags.DeleteFeatureFlagOverride;

public sealed class DeleteFeatureFlagOverrideCommandHandler : IRequestHandler<DeleteFeatureFlagOverrideCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public DeleteFeatureFlagOverrideCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteFeatureFlagOverrideCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.FeatureFlagOverrides
            .FirstOrDefaultAsync(
                o => o.Id == request.OverrideId && o.FlagId == request.FlagId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Feature flag override not found.");

        _db.FeatureFlagOverrides.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
