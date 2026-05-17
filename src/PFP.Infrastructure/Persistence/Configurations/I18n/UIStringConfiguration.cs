using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.I18n;

/// <summary>EF Core mapping for <see cref="UIString"/>. Maps to <c>UI_STRINGS</c>.</summary>
public sealed class UIStringConfiguration : IEntityTypeConfiguration<UIString>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UIString> builder)
    {
        builder.ToTable("ui_strings");

        builder.Property(x => x.Key).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LocaleCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);

        builder.HasIndex(x => new { x.Key, x.LocaleCode }).IsUnique();
        builder.HasIndex(x => x.LocaleCode);

        builder.HasOne(x => x.Locale)
               .WithMany(l => l.UIStrings)
               .HasForeignKey(x => x.LocaleCode)
               .HasPrincipalKey(l => l.Code)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
