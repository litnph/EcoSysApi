using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinDebtRecord"/>. Maps to <c>fin_debt_records</c>.</summary>
public sealed class FinDebtRecordConfiguration : IEntityTypeConfiguration<FinDebtRecord>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinDebtRecord> builder)
    {
        builder.Property(x => x.PersonName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PersonContact).HasMaxLength(200);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.HasOne(x => x.OriginalTransaction)
               .WithMany()
               .HasForeignKey(x => x.OriginalTxnId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
