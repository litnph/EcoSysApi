using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinMonthlyPeriodSystemManaged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_transaction_history_fin_transactions_entity_id",
                table: "fin_transaction_history");

            migrationBuilder.DropColumn(
                name: "category_breakdown",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "note",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "period_end",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "period_start",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "source_breakdown",
                table: "fin_monthly_periods");

            migrationBuilder.DropColumn(
                name: "transaction_count",
                table: "fin_monthly_periods");

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "fin_transaction_history",
                newName: "transaction_id");

            migrationBuilder.RenameIndex(
                name: "ix_fin_transaction_history_entity_id_version",
                table: "fin_transaction_history",
                newName: "ix_fin_transaction_history_transaction_id_version");

            migrationBuilder.RenameIndex(
                name: "ix_fin_transaction_history_entity_id_created_at",
                table: "fin_transaction_history",
                newName: "ix_fin_transaction_history_transaction_id_created_at");

            migrationBuilder.RenameColumn(
                name: "net_amount",
                table: "fin_monthly_periods",
                newName: "net");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "txn_date",
                table: "fin_transactions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "note",
                table: "fin_transactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "monthly_period_id",
                table: "fin_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_monthly_period_id",
                table: "fin_transactions",
                column: "monthly_period_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_source_id",
                table: "fin_transactions",
                column: "source_id");

            migrationBuilder.AddForeignKey(
                name: "fk_fin_transaction_history_fin_transactions_transaction_id",
                table: "fin_transaction_history",
                column: "transaction_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_transactions_fin_monthly_periods_monthly_period_id",
                table: "fin_transactions",
                column: "monthly_period_id",
                principalTable: "fin_monthly_periods",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_transaction_history_fin_transactions_transaction_id",
                table: "fin_transaction_history");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_monthly_periods_monthly_period_id",
                table: "fin_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_transactions_monthly_period_id",
                table: "fin_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_transactions_source_id",
                table: "fin_transactions");

            migrationBuilder.DropColumn(
                name: "monthly_period_id",
                table: "fin_transactions");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "fin_transaction_history",
                newName: "entity_id");

            migrationBuilder.RenameIndex(
                name: "ix_fin_transaction_history_transaction_id_version",
                table: "fin_transaction_history",
                newName: "ix_fin_transaction_history_entity_id_version");

            migrationBuilder.RenameIndex(
                name: "ix_fin_transaction_history_transaction_id_created_at",
                table: "fin_transaction_history",
                newName: "ix_fin_transaction_history_entity_id_created_at");

            migrationBuilder.RenameColumn(
                name: "net",
                table: "fin_monthly_periods",
                newName: "net_amount");

            migrationBuilder.AlterColumn<DateTime>(
                name: "txn_date",
                table: "fin_transactions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "note",
                table: "fin_transactions",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category_breakdown",
                table: "fin_monthly_periods",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "fin_monthly_periods",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "fin_monthly_periods",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                table: "fin_monthly_periods",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "fin_monthly_periods",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "fin_monthly_periods",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "period_end",
                table: "fin_monthly_periods",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "period_start",
                table: "fin_monthly_periods",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "source_breakdown",
                table: "fin_monthly_periods",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "transaction_count",
                table: "fin_monthly_periods",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_transaction_history_fin_transactions_entity_id",
                table: "fin_transaction_history",
                column: "entity_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
