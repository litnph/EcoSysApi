using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_MonthlyPeriodReportJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category_breakdown",
                table: "fin_monthly_periods",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_breakdown",
                table: "fin_monthly_periods",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category_breakdown",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "source_breakdown",
                table: "fin_monthly_periods");
        }
    }
}
