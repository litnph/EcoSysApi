using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.History;

/// <summary>EF Core mapping for <see cref="SpaceMemberHistory"/>. Maps to <c>SPACE_MEMBERS_HISTORY</c>.</summary>
public sealed class SpaceMemberHistoryConfiguration : VersionHistoryEntityConfiguration<SpaceMemberHistory>
{
    /// <inheritdoc/>
    public override void Configure(EntityTypeBuilder<SpaceMemberHistory> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Entity)
               .WithMany(m => m.History)
               .HasForeignKey(x => x.EntityId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
