using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_InstallmentPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_installment_pays_fin_billing_cycles_billing_cycle_id",
                table: "fin_installment_pays");

            migrationBuilder.DropIndex(
                name: "ix_fin_installment_plans_origin_transaction_id",
                table: "fin_installment_plans");

            migrationBuilder.DropIndex(
                name: "ix_fin_installment_plans_source_id_status",
                table: "fin_installment_plans");

            migrationBuilder.DropIndex(
                name: "ix_fin_installment_pays_billing_cycle_id",
                table: "fin_installment_pays");

            migrationBuilder.DropIndex(
                name: "ix_fin_installment_pays_status_due_date",
                table: "fin_installment_pays");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "fin_installment_plans");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "fin_installment_plans");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "fin_installment_plans");

            migrationBuilder.DropColumn(
                name: "end_date",
                table: "fin_installment_plans");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "fin_installment_plans");

            migrationBuilder.DropColumn(
                name: "paid_months",
                table: "fin_installment_plans");

            migrationBuilder.DropColumn(
                name: "billing_cycle_id",
                table: "fin_installment_pays");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "fin_installment_pays");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "fin_installment_pays");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "fin_installment_pays");

            migrationBuilder.DropColumn(
                name: "interest_amount",
                table: "fin_installment_pays");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "fin_installment_pays");

            migrationBuilder.DropColumn(
                name: "note",
                table: "fin_installment_pays");

            migrationBuilder.DropColumn(
                name: "principal_amount",
                table: "fin_installment_pays");

            migrationBuilder.AddColumn<decimal>(
                name: "paid_amount",
                table: "fin_installment_pays",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.RenameColumn(
                name: "principal",
                table: "fin_installment_plans",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "sequence_no",
                table: "fin_installment_pays",
                newName: "installment_number");

            migrationBuilder.RenameColumn(
                name: "interest_rate_pct",
                table: "fin_installment_plans",
                newName: "interest_rate");

            migrationBuilder.RenameIndex(
                name: "ix_fin_installment_pays_installment_plan_id_sequence_no",
                table: "fin_installment_pays",
                newName: "ix_fin_installment_pays_installment_plan_id_installment_number");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "start_date",
                table: "fin_installment_plans",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "conversion_fee_status",
                table: "fin_installment_plans",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "conversion_fee_rate",
                table: "fin_installment_plans",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,4)",
                oldPrecision: 7,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "conversion_fee_amount",
                table: "fin_installment_plans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<Guid>(
                name: "conversion_fee_txn_id",
                table: "fin_installment_plans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "interest_rate",
                table: "fin_installment_plans",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,4)",
                oldPrecision: 7,
                oldScale: 4);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "due_date",
                table: "fin_installment_pays",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_conversion_fee_txn_id",
                table: "fin_installment_plans",
                column: "conversion_fee_txn_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_origin_transaction_id",
                table: "fin_installment_plans",
                column: "origin_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_source_id",
                table: "fin_installment_plans",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_pays_installment_plan_id_status",
                table: "fin_installment_pays",
                columns: new[] { "installment_plan_id", "status" });

            migrationBuilder.Sql(
                """
                UPDATE fin_installment_pays SET status = 'upcoming' WHERE status = 'pending';
                UPDATE fin_installment_pays SET status = 'due' WHERE status = 'billed';
                UPDATE fin_installment_plans SET conversion_fee_status = NULL WHERE conversion_fee_status = 'waived';
                """);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_installment_plans_fin_transactions_conversion_fee_txn_id",
                table: "fin_installment_plans",
                column: "conversion_fee_txn_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_installment_plans_fin_transactions_conversion_fee_txn_id",
                table: "fin_installment_plans");

            migrationBuilder.DropIndex(
                name: "ix_fin_installment_plans_conversion_fee_txn_id",
                table: "fin_installment_plans");

            migrationBuilder.DropIndex(
                name: "ix_fin_installment_plans_origin_transaction_id",
                table: "fin_installment_plans");

            migrationBuilder.DropIndex(
                name: "ix_fin_installment_plans_source_id",
                table: "fin_installment_plans");

            migrationBuilder.DropIndex(
                name: "ix_fin_installment_pays_installment_plan_id_status",
                table: "fin_installment_pays");

            migrationBuilder.DropColumn(
                name: "conversion_fee_txn_id",
                table: "fin_installment_plans");

            migrationBuilder.DropColumn(
                name: "interest_rate",
                table: "fin_installment_plans");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "fin_installment_plans",
                newName: "principal");

            migrationBuilder.RenameColumn(
                name: "paid_amount",
                table: "fin_installment_pays",
                newName: "principal_amount");

            migrationBuilder.RenameColumn(
                name: "installment_number",
                table: "fin_installment_pays",
                newName: "sequence_no");

            migrationBuilder.RenameIndex(
                name: "ix_fin_installment_pays_installment_plan_id_installment_number",
                table: "fin_installment_pays",
                newName: "ix_fin_installment_pays_installment_plan_id_sequence_no");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "fin_installment_plans",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "conversion_fee_status",
                table: "fin_installment_plans",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "conversion_fee_rate",
                table: "fin_installment_plans",
                type: "numeric(7,4)",
                precision: 7,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,4)",
                oldPrecision: 5,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "conversion_fee_amount",
                table: "fin_installment_plans",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cancelled_at",
                table: "fin_installment_plans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "fin_installment_plans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "fin_installment_plans",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                table: "fin_installment_plans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "interest_rate_pct",
                table: "fin_installment_plans",
                type: "numeric(7,4)",
                precision: 7,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "fin_installment_plans",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "paid_months",
                table: "fin_installment_plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "due_date",
                table: "fin_installment_pays",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "billing_cycle_id",
                table: "fin_installment_pays",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "fin_installment_pays",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "fin_installment_pays",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "fin_installment_pays",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "interest_amount",
                table: "fin_installment_pays",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "fin_installment_pays",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "fin_installment_pays",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_origin_transaction_id",
                table: "fin_installment_plans",
                column: "origin_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_source_id_status",
                table: "fin_installment_plans",
                columns: new[] { "source_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_pays_billing_cycle_id",
                table: "fin_installment_pays",
                column: "billing_cycle_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_pays_status_due_date",
                table: "fin_installment_pays",
                columns: new[] { "status", "due_date" });

            migrationBuilder.AddForeignKey(
                name: "fk_fin_installment_pays_fin_billing_cycles_billing_cycle_id",
                table: "fin_installment_pays",
                column: "billing_cycle_id",
                principalTable: "fin_billing_cycles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
