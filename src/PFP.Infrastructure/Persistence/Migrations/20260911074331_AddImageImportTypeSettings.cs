using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImageImportTypeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_import_type_settings_json",
                table: "user_profiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_import_type_settings_json",
                table: "user_profiles");
        }
    }
}
