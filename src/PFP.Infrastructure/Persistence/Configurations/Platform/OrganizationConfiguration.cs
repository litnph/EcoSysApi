using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Platform;

/// <summary>EF Core mapping for <see cref="Organization"/>. Maps to <c>ORGANIZATIONS</c>.</summary>
public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.Property(x => x.Slug).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.DefaultCurrency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1024);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.OwnerId);

        builder.HasOne(x => x.Owner)
               .WithMany(u => u.OwnedOrganizations)
               .HasForeignKey(x => x.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
