using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Platform;

/// <summary>EF Core mapping for <see cref="SpaceMember"/>. Maps to <c>SPACE_MEMBERS</c>.</summary>
public sealed class SpaceMemberConfiguration : IEntityTypeConfiguration<SpaceMember>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SpaceMember> builder)
    {
        builder.HasIndex(x => new { x.SpaceId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.InheritedFromSpaceId);

        builder.HasOne(x => x.Space)
               .WithMany(s => s.Members)
               .HasForeignKey(x => x.SpaceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany(u => u.SpaceMemberships)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InheritedFromSpace)
               .WithMany()
               .HasForeignKey(x => x.InheritedFromSpaceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
