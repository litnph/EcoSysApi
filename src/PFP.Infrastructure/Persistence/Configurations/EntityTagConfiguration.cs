using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

/// <summary><c>entity_tags</c> mapping.</summary>
public sealed class EntityTagConfiguration : IEntityTypeConfiguration<EntityTag>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<EntityTag> builder)
    {
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.TagId, x.EntityType, x.EntityId })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");

        builder.HasIndex(x => new { x.EntityType, x.EntityId });

        builder.HasOne(x => x.Tag)
               .WithMany(t => t.EntityTags)
               .HasForeignKey(x => x.TagId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Tagger)
               .WithMany()
               .HasForeignKey(x => x.TaggedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
