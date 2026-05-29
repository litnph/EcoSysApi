using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BillingCycleItemsJunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "issuer_statement_amount",
                table: "fin_billing_cycles",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_refreshed_at",
                table: "fin_billing_cycles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reconciliation_note",
                table: "fin_billing_cycles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fin_billing_cycle_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    billing_cycle_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    inclusion_source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    removed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_billing_cycle_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_billing_cycle_items_fin_billing_cycles_billing_cycle_id",
                        column: x => x.billing_cycle_id,
                        principalTable: "fin_billing_cycles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_fin_billing_cycle_items_fin_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "fin_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fin_billing_cycle_items_billing_cycle_id",
                table: "fin_billing_cycle_items",
                column: "billing_cycle_id");

            migrationBuilder.Sql(
                """
                INSERT INTO fin_billing_cycle_items (
                    id, billing_cycle_id, transaction_id, inclusion_source, removed_at, created_at, updated_at)
                SELECT
                    NEWID(),
                    t.billing_cycle_id,
                    t.id,
                    N'refresh',
                    NULL,
                    t.created_at,
                    t.updated_at
                FROM fin_transactions t
                WHERE t.billing_cycle_id IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                table: "fin_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_transactions_billing_cycle_id",
                table: "fin_transactions");

            migrationBuilder.DropColumn(
                name: "billing_cycle_id",
                table: "fin_transactions");

            migrationBuilder.CreateIndex(
                name: "ux_fin_billing_cycle_items_transaction_id_active",
                table: "fin_billing_cycle_items",
                column: "transaction_id",
                unique: true,
                filter: "[removed_at] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_fin_billing_cycle_items_transaction_id_active",
                table: "fin_billing_cycle_items");

            migrationBuilder.AddColumn<Guid>(
                name: "billing_cycle_id",
                table: "fin_transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE t
                SET t.billing_cycle_id = i.billing_cycle_id
                FROM fin_transactions t
                INNER JOIN fin_billing_cycle_items i
                    ON i.transaction_id = t.id AND i.removed_at IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_billing_cycle_id",
                table: "fin_transactions",
                column: "billing_cycle_id");

            migrationBuilder.AddForeignKey(
                name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                table: "fin_transactions",
                column: "billing_cycle_id",
                principalTable: "fin_billing_cycles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropTable(
                name: "fin_billing_cycle_items");

            migrationBuilder.DropColumn(
                name: "issuer_statement_amount",
                table: "fin_billing_cycles");

            migrationBuilder.DropColumn(
                name: "last_refreshed_at",
                table: "fin_billing_cycles");

            migrationBuilder.DropColumn(
                name: "reconciliation_note",
                table: "fin_billing_cycles");
        }
    }
}
