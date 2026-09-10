using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

public sealed class TransactionClassificationRuleConfiguration
    : IEntityTypeConfiguration<TransactionClassificationRule>
{
    public void Configure(EntityTypeBuilder<TransactionClassificationRule> builder)
    {
        builder.Property(x => x.Keyword).HasMaxLength(120).IsRequired();
        builder.Property(x => x.NormalizedKeyword).HasMaxLength(120).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.NormalizedKeyword })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
        builder.HasIndex(x => new { x.UserId, x.IsActive });

        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
