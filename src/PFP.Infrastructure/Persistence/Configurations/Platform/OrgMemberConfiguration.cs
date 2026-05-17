using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Platform;

/// <summary>EF Core mapping for <see cref="OrgMember"/>. Maps to <c>ORG_MEMBERS</c>.</summary>
public sealed class OrgMemberConfiguration : IEntityTypeConfiguration<OrgMember>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<OrgMember> builder)
    {
        builder.HasIndex(x => new { x.OrgId, x.UserId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.IsActive });

        builder.HasOne(x => x.Org)
               .WithMany(o => o.Members)
               .HasForeignKey(x => x.OrgId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany(u => u.OrgMemberships)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
