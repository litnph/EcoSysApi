using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

public sealed class ImageImportTypeConfigurationConfiguration
    : IEntityTypeConfiguration<ImageImportTypeConfiguration>
{
    public void Configure(EntityTypeBuilder<ImageImportTypeConfiguration> builder)
    {
        builder.ToTable("image_import_type_settings");
        builder.Property(x => x.Type).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => x.Type).IsUnique();
        builder.HasOne(x => x.Source)
            .WithMany()
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
