using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.History;

/// <summary>EF Core mapping for <see cref="FinSourceHistory"/>. Maps to <c>FIN_SOURCES_HISTORY</c>.</summary>
public sealed class FinSourceHistoryConfiguration : VersionHistoryEntityConfiguration<FinSourceHistory>
{
    /// <inheritdoc/>
    public override void Configure(EntityTypeBuilder<FinSourceHistory> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Entity)
               .WithMany(s => s.History)
               .HasForeignKey(x => x.EntityId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
