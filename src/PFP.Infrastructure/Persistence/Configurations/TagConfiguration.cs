using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

/// <summary><c>TAGS</c> → <c>tags</c>.</summary>
public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(7).IsRequired();

        builder.HasIndex(x => new { x.SmoduleId, x.Name })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");

        builder.HasOne(x => x.Smodule)
               .WithMany()
               .HasForeignKey(x => x.SmoduleId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
