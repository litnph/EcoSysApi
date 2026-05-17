using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="FeatureFlagOverride"/> to <c>FEATURE_FLAG_OVERRIDES</c> (PostgreSQL → <c>feature_flag_overrides</c>).</summary>
public sealed class FeatureFlagOverrideConfiguration : IEntityTypeConfiguration<FeatureFlagOverride>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FeatureFlagOverride> builder)
    {
        builder.HasIndex(x => new { x.FlagId, x.TargetType, x.TargetId });
        builder.HasIndex(x => x.ExpiresAt);
    }
}
