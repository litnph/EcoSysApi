using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyReportPeriodMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "monthly_report_period_mode",
                table: "user_profiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "lower_boundary");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_profiles_monthly_report_period_mode",
                table: "user_profiles",
                sql: "monthly_report_period_mode IN ('lower_boundary', 'upper_boundary')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_user_profiles_monthly_report_period_mode",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "monthly_report_period_mode",
                table: "user_profiles");
        }
    }
}
