using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinCategory"/>. Maps to <c>FIN_CATEGORIES</c>.</summary>
public sealed class FinCategoryConfiguration : IEntityTypeConfiguration<FinCategory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinCategory> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(64);
        builder.Property(x => x.Color).HasMaxLength(16);
        builder.Property(x => x.Path).HasMaxLength(2048);
        builder.Property(x => x.Description).HasMaxLength(1024);

        builder.HasIndex(x => new { x.SmoduleId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.SmoduleId, x.Kind });
        builder.HasIndex(x => new { x.SmoduleId, x.ParentId });

        builder.HasOne(x => x.Smodule)
               .WithMany(m => m.FinCategories)
               .HasForeignKey(x => x.SmoduleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Parent)
               .WithMany(c => c.Children)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
