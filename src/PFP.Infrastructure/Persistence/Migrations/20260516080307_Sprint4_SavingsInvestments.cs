using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4_SavingsInvestments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_investment_txns_fin_transactions_transaction_id",
                table: "fin_investment_txns");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_investments_fin_sources_source_id",
                table: "fin_investments");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_savings_fin_sources_source_id",
                table: "fin_savings");

            migrationBuilder.DropIndex(
                name: "ix_fin_savings_maturity_date",
                table: "fin_savings");

            migrationBuilder.DropIndex(
                name: "ix_fin_investments_smodule_id_is_closed",
                table: "fin_investments");

            migrationBuilder.DropIndex(
                name: "ix_fin_investments_source_id",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "account_number",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "accrued_interest",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "auto_rollover",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "bank_name",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "expected_return_amount",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "term_months",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "withdrawn_at",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "avg_cost",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "broker_name",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "closed_at",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "current_price",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "is_closed",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "price_updated_at",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "source_id",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "symbol",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "total_dividends",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "total_fees",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "fin_investment_txns");

            migrationBuilder.DropColumn(
                name: "fee",
                table: "fin_investment_txns");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "fin_investment_txns");

            migrationBuilder.DropColumn(
                name: "realised_pn_l",
                table: "fin_investment_txns");

            migrationBuilder.DropColumn(
                name: "tax",
                table: "fin_investment_txns");

            migrationBuilder.RenameColumn(
                name: "principal",
                table: "fin_savings",
                newName: "current_amount");

            migrationBuilder.AddColumn<decimal>(
                name: "target_amount",
                table: "fin_savings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.DropColumn(
                name: "withdrawn_amount",
                table: "fin_savings");

            migrationBuilder.RenameColumn(
                name: "total_proceeds",
                table: "fin_investments",
                newName: "total_returned");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "fin_investment_txns",
                newName: "linked_txn_id");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "fin_investment_txns",
                newName: "amount");

            migrationBuilder.RenameIndex(
                name: "ix_fin_investment_txns_transaction_id",
                table: "fin_investment_txns",
                newName: "ix_fin_investment_txns_linked_txn_id");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "start_date",
                table: "fin_savings",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.Sql("DELETE FROM fin_savings WHERE source_id IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "source_id",
                table: "fin_savings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "fin_savings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "maturity_date",
                table: "fin_savings",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<decimal>(
                name: "interest_rate",
                table: "fin_savings",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE fin_savings
                SET interest_rate = COALESCE(interest_rate_pct, 0)::numeric(5,4)
                """);

            migrationBuilder.DropColumn(
                name: "interest_rate_pct",
                table: "fin_savings");

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "fin_savings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "fin_savings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "fin_investments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.Sql(
                """
                UPDATE fin_investments SET currency = LEFT(currency, 3) WHERE length(currency) > 3;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "fin_investments",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "fin_investments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "txn_date",
                table: "fin_investment_txns",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "quantity",
                table: "fin_investment_txns",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "price_per_unit",
                table: "fin_investment_txns",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "fin_investment_txns",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE fin_savings SET status = 'withdrawn' WHERE status = 'closed_early';
                UPDATE fin_savings SET type = 'flexible' WHERE type = '';
                UPDATE fin_investments SET type = 'other' WHERE type IN ('bond', 'commodity');
                DELETE FROM fin_investment_txns WHERE txn_type IN ('split', 'reinvest');
                """);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_investment_txns_fin_transactions_linked_txn_id",
                table: "fin_investment_txns",
                column: "linked_txn_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_savings_fin_sources_source_id",
                table: "fin_savings",
                column: "source_id",
                principalTable: "fin_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_investment_txns_fin_transactions_linked_txn_id",
                table: "fin_investment_txns");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_savings_fin_sources_source_id",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "interest_rate",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "note",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "type",
                table: "fin_savings");

            migrationBuilder.DropColumn(
                name: "note",
                table: "fin_investments");

            migrationBuilder.DropColumn(
                name: "note",
                table: "fin_investment_txns");

            migrationBuilder.RenameColumn(
                name: "target_amount",
                table: "fin_savings",
                newName: "withdrawn_amount");

            migrationBuilder.RenameColumn(
                name: "current_amount",
                table: "fin_savings",
                newName: "principal");

            migrationBuilder.RenameColumn(
                name: "total_returned",
                table: "fin_investments",
                newName: "total_proceeds");

            migrationBuilder.RenameColumn(
                name: "linked_txn_id",
                table: "fin_investment_txns",
                newName: "transaction_id");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "fin_investment_txns",
                newName: "total_amount");

            migrationBuilder.RenameIndex(
                name: "ix_fin_investment_txns_linked_txn_id",
                table: "fin_investment_txns",
                newName: "ix_fin_investment_txns_transaction_id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "fin_savings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<Guid>(
                name: "source_id",
                table: "fin_savings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "fin_savings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "maturity_date",
                table: "fin_savings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "account_number",
                table: "fin_savings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "accrued_interest",
                table: "fin_savings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "auto_rollover",
                table: "fin_savings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "bank_name",
                table: "fin_savings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "fin_savings",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "expected_return_amount",
                table: "fin_savings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "interest_rate_pct",
                table: "fin_savings",
                type: "numeric(7,4)",
                precision: 7,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "fin_savings",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "term_months",
                table: "fin_savings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "withdrawn_at",
                table: "fin_savings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "fin_investments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "fin_investments",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<decimal>(
                name: "avg_cost",
                table: "fin_investments",
                type: "numeric(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "broker_name",
                table: "fin_investments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "closed_at",
                table: "fin_investments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "current_price",
                table: "fin_investments",
                type: "numeric(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_closed",
                table: "fin_investments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "fin_investments",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "price_updated_at",
                table: "fin_investments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity",
                table: "fin_investments",
                type: "numeric(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "source_id",
                table: "fin_investments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "fin_investments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "symbol",
                table: "fin_investments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_dividends",
                table: "fin_investments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_fees",
                table: "fin_investments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "txn_date",
                table: "fin_investment_txns",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<decimal>(
                name: "quantity",
                table: "fin_investment_txns",
                type: "numeric(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "price_per_unit",
                table: "fin_investment_txns",
                type: "numeric(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "fin_investment_txns",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "fee",
                table: "fin_investment_txns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "fin_investment_txns",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "realised_pn_l",
                table: "fin_investment_txns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tax",
                table: "fin_investment_txns",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_fin_savings_maturity_date",
                table: "fin_savings",
                column: "maturity_date");

            migrationBuilder.CreateIndex(
                name: "ix_fin_investments_smodule_id_is_closed",
                table: "fin_investments",
                columns: new[] { "smodule_id", "is_closed" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_investments_source_id",
                table: "fin_investments",
                column: "source_id");

            migrationBuilder.AddForeignKey(
                name: "fk_fin_investment_txns_fin_transactions_transaction_id",
                table: "fin_investment_txns",
                column: "transaction_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_investments_fin_sources_source_id",
                table: "fin_investments",
                column: "source_id",
                principalTable: "fin_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_savings_fin_sources_source_id",
                table: "fin_savings",
                column: "source_id",
                principalTable: "fin_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
