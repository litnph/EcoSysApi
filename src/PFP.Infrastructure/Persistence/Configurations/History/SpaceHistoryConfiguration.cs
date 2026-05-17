using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.History;

/// <summary>EF Core mapping for <see cref="SpaceHistory"/>. Maps to <c>SPACES_HISTORY</c>.</summary>
public sealed class SpaceHistoryConfiguration : VersionHistoryEntityConfiguration<SpaceHistory>
{
    /// <inheritdoc/>
    public override void Configure(EntityTypeBuilder<SpaceHistory> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Entity)
               .WithMany(s => s.History)
               .HasForeignKey(x => x.EntityId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
