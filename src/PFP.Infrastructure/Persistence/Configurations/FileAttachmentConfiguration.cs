using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="FileAttachment"/> to <c>file_attachments</c>.</summary>
public sealed class FileAttachmentConfiguration : IEntityTypeConfiguration<FileAttachment>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FileAttachment> builder)
    {
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FileKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.UploadedBy);

        builder.HasOne(x => x.Uploader)
               .WithMany()
               .HasForeignKey(x => x.UploadedBy)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
