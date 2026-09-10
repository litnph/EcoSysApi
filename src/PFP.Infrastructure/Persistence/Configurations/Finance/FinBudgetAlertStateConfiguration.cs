using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

public sealed class FinBudgetAlertStateConfiguration : IEntityTypeConfiguration<FinBudgetAlertState>
{
    public void Configure(EntityTypeBuilder<FinBudgetAlertState> builder)
    {
        builder.Property(x => x.Currency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.CycleStart).HasColumnType("date");
        builder.Property(x => x.CycleEnd).HasColumnType("date");
        builder.Property(x => x.AlertKey).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new
        {
            x.UserId,
            x.CategoryId,
            x.Currency,
            x.CycleStart,
            x.CycleEnd,
            x.ConfigurationVersion,
            x.AlertKey,
        }).IsUnique();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
