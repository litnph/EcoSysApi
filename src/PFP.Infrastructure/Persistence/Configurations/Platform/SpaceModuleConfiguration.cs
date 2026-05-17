using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Platform;

/// <summary>EF Core mapping for <see cref="SpaceModule"/>. Maps to <c>SPACE_MODULES</c>.</summary>
public sealed class SpaceModuleConfiguration : IEntityTypeConfiguration<SpaceModule>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SpaceModule> builder)
    {
        // Module-specific config as PostgreSQL jsonb so handlers can index into it.
        builder.Property(x => x.Settings).HasColumnType("jsonb");

        builder.HasIndex(x => new { x.SpaceId, x.ModuleCode }).IsUnique();

        builder.HasOne(x => x.Space)
               .WithMany(s => s.Modules)
               .HasForeignKey(x => x.SpaceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
