using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionClassificationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transaction_classification_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    keyword = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    normalized_keyword = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tag_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction_classification_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_classification_rules_fin_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "fin_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transaction_classification_rules_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transaction_classification_rules_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transaction_classification_rules_category_id",
                table: "transaction_classification_rules",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_classification_rules_tag_id",
                table: "transaction_classification_rules",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_classification_rules_user_id_is_active",
                table: "transaction_classification_rules",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_transaction_classification_rules_user_id_normalized_keyword",
                table: "transaction_classification_rules",
                columns: new[] { "user_id", "normalized_keyword" },
                unique: true,
                filter: "[is_deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction_classification_rules");
        }
    }
}
