using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Platform;

/// <summary>
/// EF Core mapping for <see cref="Space"/>. Maps to <c>SPACES</c>.
/// <para>Indexes per spec §3.8: single-column on <c>Path</c>, composite (<c>OrgId, ParentId</c>).</para>
/// </summary>
public sealed class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1024);
        builder.Property(x => x.Path).HasMaxLength(2048).IsRequired();

        // Spec §3.8.
        builder.HasIndex(x => x.Path);
        builder.HasIndex(x => new { x.OrgId, x.ParentId });

        builder.HasOne(x => x.Org)
               .WithMany(o => o.Spaces)
               .HasForeignKey(x => x.OrgId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Parent)
               .WithMany(s => s.Children)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
