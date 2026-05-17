using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="FeatureFlag"/> to <c>FEATURE_FLAGS</c> (PostgreSQL → <c>feature_flags</c>).</summary>
public sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.Key).IsUnique();

        builder.HasMany(x => x.Overrides)
               .WithOne(x => x.Flag)
               .HasForeignKey(x => x.FlagId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
