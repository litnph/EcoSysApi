using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.I18n;

/// <summary>EF Core mapping for <see cref="TranslationFallback"/>. Maps to <c>TRANSLATION_FALLBACKS</c>.</summary>
public sealed class TranslationFallbackConfiguration : IEntityTypeConfiguration<TranslationFallback>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TranslationFallback> builder)
    {
        builder.Property(x => x.LocaleCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FallbackLocaleCode).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.LocaleCode, x.Priority }).IsUnique();

        builder.HasOne(x => x.Locale)
               .WithMany(l => l.Fallbacks)
               .HasForeignKey(x => x.LocaleCode)
               .HasPrincipalKey(l => l.Code)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FallbackLocale)
               .WithMany()
               .HasForeignKey(x => x.FallbackLocaleCode)
               .HasPrincipalKey(l => l.Code)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
