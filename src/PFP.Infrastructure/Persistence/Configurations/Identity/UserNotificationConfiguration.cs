using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.Property(x => x.CategoryName).HasMaxLength(100);
        builder.Property(x => x.Currency).HasMaxLength(8);
        builder.Property(x => x.SpentAmount).HasPrecision(18, 2);
        builder.Property(x => x.BudgetAmount).HasPrecision(18, 2);
        builder.Property(x => x.UtilizationPercent).HasPrecision(9, 2);
        builder.Property(x => x.ThresholdPercent).HasPrecision(5, 2);
        builder.Property(x => x.CycleStart).HasColumnType("date");
        builder.Property(x => x.CycleEnd).HasColumnType("date");
        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
    }
}
