using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

public sealed class FinCategoryBudgetConfiguration : IEntityTypeConfiguration<FinCategoryBudget>
{
    public void Configure(EntityTypeBuilder<FinCategoryBudget> builder)
    {
        builder.Property(x => x.Currency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.TargetMode).IsRequired();
        builder.Property(x => x.WarningThresholdsJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.HasIndex(x => new { x.UserId, x.CategoryId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.IsEnabled });
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
