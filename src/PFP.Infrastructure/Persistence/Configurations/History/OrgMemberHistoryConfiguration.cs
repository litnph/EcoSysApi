using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.History;

/// <summary>EF Core mapping for <see cref="OrgMemberHistory"/>. Maps to <c>ORG_MEMBERS_HISTORY</c>.</summary>
public sealed class OrgMemberHistoryConfiguration : VersionHistoryEntityConfiguration<OrgMemberHistory>
{
    /// <inheritdoc/>
    public override void Configure(EntityTypeBuilder<OrgMemberHistory> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Entity)
               .WithMany(m => m.History)
               .HasForeignKey(x => x.EntityId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
