using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// Aligns <c>__EFMigrationsHistory</c> column names with the project's snake_case convention.
/// EF Core's default repository uses <c>MigrationId</c> / <c>ProductVersion</c>, which breaks when
/// the history table was created with <c>migration_id</c> / <c>product_version</c>.
/// </summary>
internal sealed class SnakeCaseNpgsqlHistoryRepository : NpgsqlHistoryRepository
{
    public SnakeCaseNpgsqlHistoryRepository(HistoryRepositoryDependencies dependencies)
        : base(dependencies)
    {
    }

    protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
    {
        base.ConfigureTable(history);

        history.Property(h => h.MigrationId).HasColumnName("migration_id");
        history.Property(h => h.ProductVersion).HasColumnName("product_version");
    }
}
