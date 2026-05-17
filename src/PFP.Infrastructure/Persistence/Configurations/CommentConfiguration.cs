using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

/// <summary><c>COMMENTS</c> → <c>comments</c>.</summary>
public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(x => x.ModuleCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(2000).IsRequired();

        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt });

        builder.HasOne(x => x.Author)
               .WithMany()
               .HasForeignKey(x => x.AuthorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Parent)
               .WithMany(x => x.Replies)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
