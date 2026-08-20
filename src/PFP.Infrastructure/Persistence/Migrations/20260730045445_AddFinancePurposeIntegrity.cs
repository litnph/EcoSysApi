using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancePurposeIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT linked_txn_id
                        FROM fin_investment_txns
                        WHERE linked_txn_id IS NOT NULL
                        GROUP BY linked_txn_id
                        HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Cannot enforce unique investment cash links: duplicate linked_txn_id values exist.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "ix_fin_investment_txns_linked_txn_id",
                table: "fin_investment_txns");

            migrationBuilder.CreateIndex(
                name: "ix_fin_investment_txns_linked_txn_id",
                table: "fin_investment_txns",
                column: "linked_txn_id",
                unique: true,
                filter: "linked_txn_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                table: "fin_transactions",
                column: "billing_cycle_id",
                principalTable: "fin_billing_cycles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_transactions_fin_savings_saving_id",
                table: "fin_transactions",
                column: "saving_id",
                principalTable: "fin_savings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                table: "fin_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_savings_saving_id",
                table: "fin_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_investment_txns_linked_txn_id",
                table: "fin_investment_txns");

            migrationBuilder.CreateIndex(
                name: "ix_fin_investment_txns_linked_txn_id",
                table: "fin_investment_txns",
                column: "linked_txn_id");
        }
    }
}
