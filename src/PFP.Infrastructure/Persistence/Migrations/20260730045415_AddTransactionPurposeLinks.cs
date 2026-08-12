using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionPurposeLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "billing_cycle_id",
                table: "fin_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "purpose",
                table: "fin_transactions",
                type: "text",
                nullable: false,
                defaultValue: "general");

            migrationBuilder.AddColumn<Guid>(
                name: "saving_id",
                table: "fin_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_billing_cycle_id",
                table: "fin_transactions",
                column: "billing_cycle_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_saving_id",
                table: "fin_transactions",
                column: "saving_id");

            migrationBuilder.Sql(
                """
                UPDATE fin_transactions
                SET purpose = 'statement_payment'
                WHERE note = 'Thanh toán kỳ sao kê'
                   OR description = 'Thanh toán kỳ sao kê';

                UPDATE fin_transactions
                SET purpose = CASE WHEN amount < 0 THEN 'saving_deposit' ELSE 'saving_withdrawal' END,
                    saving_id = CASE
                        WHEN substring(external_ref from 8) ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
                        THEN substring(external_ref from 8)::uuid
                        ELSE NULL
                    END
                WHERE type = 'transfer'
                  AND external_ref LIKE 'saving:%';

                UPDATE fin_transactions
                SET purpose = 'installment_payment'
                WHERE type = 'direct'
                  AND installment_plan_id IS NOT NULL;

                UPDATE fin_transactions AS txn
                SET purpose = 'conversion_fee'
                WHERE EXISTS (
                    SELECT 1
                    FROM fin_installment_plans AS plan
                    WHERE plan.conversion_fee_txn_id = txn.id
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_fin_transactions_billing_cycle_id",
                table: "fin_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_transactions_saving_id",
                table: "fin_transactions");

            migrationBuilder.DropColumn(
                name: "billing_cycle_id",
                table: "fin_transactions");

            migrationBuilder.DropColumn(
                name: "purpose",
                table: "fin_transactions");

            migrationBuilder.DropColumn(
                name: "saving_id",
                table: "fin_transactions");

        }
    }
}
