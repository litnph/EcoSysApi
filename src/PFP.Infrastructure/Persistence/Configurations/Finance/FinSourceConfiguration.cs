using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinSource"/>. Maps to <c>FIN_SOURCES</c>.</summary>
public sealed class FinSourceConfiguration : IEntityTypeConfiguration<FinSource>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinSource> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(64);
        builder.Property(x => x.Color).HasMaxLength(16);
        builder.Property(x => x.Description).HasMaxLength(1024);
        builder.Property(x => x.ExternalRef).HasMaxLength(255);    }
}
