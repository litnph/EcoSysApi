using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_BillingCycles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                table: "fin_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_billing_cycles_payment_due_date",
                table: "fin_billing_cycles");

            migrationBuilder.DropIndex(
                name: "ix_fin_billing_cycles_smodule_id_status",
                table: "fin_billing_cycles");

            migrationBuilder.DropIndex(
                name: "ix_fin_billing_cycles_source_id_period_start",
                table: "fin_billing_cycles");

            migrationBuilder.DropIndex(
                name: "ix_fin_billing_cycles_source_id_period_start_period_end",
                table: "fin_billing_cycles");

            migrationBuilder.DropColumn(
                name: "closing_balance",
                table: "fin_billing_cycles");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "fin_billing_cycles");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "fin_billing_cycles");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "fin_billing_cycles");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "fin_billing_cycles");

            migrationBuilder.DropColumn(
                name: "minimum_payment",
                table: "fin_billing_cycles");

            migrationBuilder.DropColumn(
                name: "opening_balance",
                table: "fin_billing_cycles");

            migrationBuilder.RenameColumn(
                name: "total_spend",
                table: "fin_billing_cycles",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "total_payments",
                table: "fin_billing_cycles",
                newName: "paid_amount");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "statement_date",
                table: "fin_billing_cycles",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "period_start",
                table: "fin_billing_cycles",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "period_end",
                table: "fin_billing_cycles",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "payment_due_date",
                table: "fin_billing_cycles",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "ix_fin_billing_cycles_smodule_id",
                table: "fin_billing_cycles",
                column: "smodule_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_billing_cycles_source_id_period_start",
                table: "fin_billing_cycles",
                columns: new[] { "source_id", "period_start" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_billing_cycles_source_id_period_start_period_end",
                table: "fin_billing_cycles",
                columns: new[] { "source_id", "period_start", "period_end" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_billing_cycles_source_id_status",
                table: "fin_billing_cycles",
                columns: new[] { "source_id", "status" });

            migrationBuilder.AddForeignKey(
                name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                table: "fin_transactions",
                column: "billing_cycle_id",
                principalTable: "fin_billing_cycles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                table: "fin_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_billing_cycles_smodule_id",
                table: "fin_billing_cycles");

            migrationBuilder.DropIndex(
                name: "ix_fin_billing_cycles_source_id_period_start",
                table: "fin_billing_cycles");

            migrationBuilder.DropIndex(
                name: "ix_fin_billing_cycles_source_id_period_start_period_end",
                table: "fin_billing_cycles");

            migrationBuilder.DropIndex(
                name: "ix_fin_billing_cycles_source_id_status",
                table: "fin_billing_cycles");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "fin_billing_cycles",
                newName: "total_spend");

            migrationBuilder.RenameColumn(
                name: "paid_amount",
                table: "fin_billing_cycles",
                newName: "total_payments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "statement_date",
                table: "fin_billing_cycles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "period_start",
                table: "fin_billing_cycles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "period_end",
                table: "fin_billing_cycles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "payment_due_date",
                table: "fin_billing_cycles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<decimal>(
                name: "closing_balance",
                table: "fin_billing_cycles",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "fin_billing_cycles",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "fin_billing_cycles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "fin_billing_cycles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "fin_billing_cycles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "minimum_payment",
                table: "fin_billing_cycles",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "opening_balance",
                table: "fin_billing_cycles",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_fin_billing_cycles_payment_due_date",
                table: "fin_billing_cycles",
                column: "payment_due_date");

            migrationBuilder.CreateIndex(
                name: "ix_fin_billing_cycles_smodule_id_status",
                table: "fin_billing_cycles",
                columns: new[] { "smodule_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_billing_cycles_source_id_period_start",
                table: "fin_billing_cycles",
                columns: new[] { "source_id", "period_start" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_billing_cycles_source_id_period_start_period_end",
                table: "fin_billing_cycles",
                columns: new[] { "source_id", "period_start", "period_end" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                table: "fin_transactions",
                column: "billing_cycle_id",
                principalTable: "fin_billing_cycles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
