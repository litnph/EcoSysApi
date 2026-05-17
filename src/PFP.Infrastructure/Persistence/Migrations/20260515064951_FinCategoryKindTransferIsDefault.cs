using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinCategoryKindTransferIsDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE fin_categories SET kind = 'transfer' WHERE kind = 'both';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "fin_categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<bool>(
                name: "is_default",
                table: "fin_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_fin_categories_smodule_id_parent_id",
                table: "fin_categories",
                columns: new[] { "smodule_id", "parent_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE fin_categories SET kind = 'both' WHERE kind = 'transfer';
                """);

            migrationBuilder.DropIndex(
                name: "ix_fin_categories_smodule_id_parent_id",
                table: "fin_categories");

            migrationBuilder.DropColumn(
                name: "is_default",
                table: "fin_categories");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "fin_categories",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
