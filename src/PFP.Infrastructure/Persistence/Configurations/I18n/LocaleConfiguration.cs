using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.I18n;

/// <summary>EF Core mapping for <see cref="Locale"/>. Maps to <c>LOCALES</c>.</summary>
public sealed class LocaleConfiguration : IEntityTypeConfiguration<Locale>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Locale> builder)
    {
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EnglishName).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        // At most one default locale at any time — partial unique index (spec §3.4).
        builder.HasIndex(x => x.IsDefault)
               .IsUnique()
               .HasFilter("is_default = true")
               .HasDatabaseName("ix_locales_is_default_singleton");
    }
}
