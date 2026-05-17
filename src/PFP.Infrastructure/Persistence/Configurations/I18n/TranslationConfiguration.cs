using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.I18n;

/// <summary>
/// EF Core mapping for <see cref="Translation"/>. Maps to <c>TRANSLATIONS</c>.
/// <para>Index <c>(EntityType, EntityId, LocaleCode)</c> per spec §3.8.</para>
/// </summary>
public sealed class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Field).HasMaxLength(64).IsRequired();
        builder.Property(x => x.LocaleCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Value).IsRequired();

        // Spec §3.8.
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.LocaleCode });
        // Natural-key uniqueness — one translation per (entity, field, locale).
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.Field, x.LocaleCode }).IsUnique();

        builder.HasOne(x => x.Locale)
               .WithMany(l => l.Translations)
               .HasForeignKey(x => x.LocaleCode)
               .HasPrincipalKey(l => l.Code)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
