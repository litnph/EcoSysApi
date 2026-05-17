using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.FeatureFlags.CreateFeatureFlag;

public sealed class CreateFeatureFlagCommandHandler : IRequestHandler<CreateFeatureFlagCommand, CreateFeatureFlagResponse>
{
    private readonly IApplicationDbContext _db;

    public CreateFeatureFlagCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<CreateFeatureFlagResponse> Handle(CreateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var taken = await _db.FeatureFlags.AnyAsync(f => f.Key == request.Key, cancellationToken).ConfigureAwait(false);
        if (taken)
            throw new BusinessRuleException("Feature flag key already exists.");

        var entity = new FeatureFlag
        {
            Key = request.Key.Trim(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsEnabledGlobal = request.IsEnabledGlobal,
            RolloutPercentage = request.RolloutPercentage,
            IsArchived = request.IsArchived,
        };

        _db.FeatureFlags.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateFeatureFlagResponse(entity.Id);
    }
}
