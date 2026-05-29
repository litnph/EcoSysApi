using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixMonthlyReportSnapshotColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_refreshed_at",
                table: "fin_monthly_periods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "report_created_at",
                table: "fin_monthly_periods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "report_snapshot",
                table: "fin_monthly_periods",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_refreshed_at",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "report_created_at",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "report_snapshot",
                table: "fin_monthly_periods");
        }
    }
}
