using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_DebtRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_debt_records_fin_transactions_origin_transaction_id",
                table: "fin_debt_records");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_debt_records_users_counterparty_user_id",
                table: "fin_debt_records");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_debt_transactions_fin_transactions_transaction_id",
                table: "fin_debt_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_debt_records_debt_record_id",
                table: "fin_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_transactions_debt_record_id",
                table: "fin_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_debt_records_due_date",
                table: "fin_debt_records");

            migrationBuilder.DropIndex(
                name: "ix_fin_debt_records_origin_transaction_id",
                table: "fin_debt_records");

            migrationBuilder.DropIndex(
                name: "ix_fin_debt_records_counterparty_user_id",
                table: "fin_debt_records");

            migrationBuilder.DropColumn(
                name: "debt_record_id",
                table: "fin_transactions");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "fin_debt_transactions");

            migrationBuilder.DropColumn(
                name: "remaining_after",
                table: "fin_debt_transactions");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "fin_debt_transactions",
                newName: "note");

            migrationBuilder.RenameColumn(
                name: "txn_type",
                table: "fin_debt_transactions",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "fin_debt_transactions",
                newName: "txn_id");

            migrationBuilder.RenameIndex(
                name: "ix_fin_debt_transactions_transaction_id",
                table: "fin_debt_transactions",
                newName: "ix_fin_debt_transactions_txn_id");

            migrationBuilder.RenameColumn(
                name: "principal_amount",
                table: "fin_debt_records",
                newName: "original_amount");

            migrationBuilder.RenameColumn(
                name: "counterparty_name",
                table: "fin_debt_records",
                newName: "person_name");

            migrationBuilder.RenameColumn(
                name: "counterparty_contact",
                table: "fin_debt_records",
                newName: "person_contact");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "fin_debt_records",
                newName: "note");

            migrationBuilder.AddColumn<Guid>(
                name: "original_txn_id",
                table: "fin_debt_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE fin_debt_records SET original_txn_id = origin_transaction_id
                WHERE origin_transaction_id IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "origin_transaction_id",
                table: "fin_debt_records");

            migrationBuilder.DropColumn(
                name: "counterparty_user_id",
                table: "fin_debt_records");

            migrationBuilder.DropColumn(
                name: "interest_rate_pct",
                table: "fin_debt_records");

            migrationBuilder.DropColumn(
                name: "settled_at",
                table: "fin_debt_records");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "fin_debt_records");

            migrationBuilder.Sql(
                """
                UPDATE fin_debt_records SET status = CASE status
                    WHEN 'open' THEN 'active'
                    WHEN 'partially_paid' THEN 'active'
                    WHEN 'settled' THEN 'completed'
                    WHEN 'written_off' THEN 'cancelled'
                    ELSE status
                END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE fin_debt_transactions SET type = CASE type
                    WHEN 'payment' THEN 'payment'
                    WHEN 'interest' THEN 'payment'
                    WHEN 'fee' THEN 'payment'
                    WHEN 'adjustment' THEN 'payment'
                    WHEN 'write_off' THEN 'payment'
                    ELSE type
                END;
                """);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "txn_date",
                table: "fin_debt_transactions",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "note",
                table: "fin_debt_transactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "due_date",
                table: "fin_debt_records",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "fin_debt_records",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "person_name",
                table: "fin_debt_records",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "person_contact",
                table: "fin_debt_records",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "note",
                table: "fin_debt_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_smodule_id_due_date",
                table: "fin_debt_records",
                columns: new[] { "smodule_id", "due_date" });

            migrationBuilder.AddForeignKey(
                name: "fk_fin_debt_records_fin_transactions_original_txn_id",
                table: "fin_debt_records",
                column: "original_txn_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_debt_transactions_fin_transactions_txn_id",
                table: "fin_debt_transactions",
                column: "txn_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_debt_records_fin_transactions_original_txn_id",
                table: "fin_debt_records");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_debt_transactions_fin_transactions_txn_id",
                table: "fin_debt_transactions");

            migrationBuilder.DropIndex(
                name: "ix_fin_debt_records_smodule_id_due_date",
                table: "fin_debt_records");

            migrationBuilder.DropColumn(
                name: "original_txn_id",
                table: "fin_debt_records");

            migrationBuilder.RenameColumn(
                name: "note",
                table: "fin_debt_transactions",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "fin_debt_transactions",
                newName: "txn_type");

            migrationBuilder.RenameColumn(
                name: "txn_id",
                table: "fin_debt_transactions",
                newName: "transaction_id");

            migrationBuilder.RenameIndex(
                name: "ix_fin_debt_transactions_txn_id",
                table: "fin_debt_transactions",
                newName: "ix_fin_debt_transactions_transaction_id");

            migrationBuilder.RenameColumn(
                name: "original_amount",
                table: "fin_debt_records",
                newName: "principal_amount");

            migrationBuilder.RenameColumn(
                name: "person_name",
                table: "fin_debt_records",
                newName: "counterparty_name");

            migrationBuilder.RenameColumn(
                name: "person_contact",
                table: "fin_debt_records",
                newName: "counterparty_contact");

            migrationBuilder.RenameColumn(
                name: "note",
                table: "fin_debt_records",
                newName: "notes");

            migrationBuilder.AddColumn<Guid>(
                name: "debt_record_id",
                table: "fin_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "origin_transaction_id",
                table: "fin_debt_records",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "counterparty_user_id",
                table: "fin_debt_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "interest_rate_pct",
                table: "fin_debt_records",
                type: "numeric(7,4)",
                precision: 7,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "settled_at",
                table: "fin_debt_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "fin_debt_records",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "txn_date",
                table: "fin_debt_transactions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "fin_debt_transactions",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "remaining_after",
                table: "fin_debt_transactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "due_date",
                table: "fin_debt_records",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "fin_debt_records",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "counterparty_name",
                table: "fin_debt_records",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "counterparty_contact",
                table: "fin_debt_records",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "notes",
                table: "fin_debt_records",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_debt_record_id",
                table: "fin_transactions",
                column: "debt_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_due_date",
                table: "fin_debt_records",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_origin_transaction_id",
                table: "fin_debt_records",
                column: "origin_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_counterparty_user_id",
                table: "fin_debt_records",
                column: "counterparty_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_fin_debt_records_fin_transactions_origin_transaction_id",
                table: "fin_debt_records",
                column: "origin_transaction_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_debt_records_users_counterparty_user_id",
                table: "fin_debt_records",
                column: "counterparty_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_debt_transactions_fin_transactions_transaction_id",
                table: "fin_debt_transactions",
                column: "transaction_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_transactions_fin_debt_records_debt_record_id",
                table: "fin_transactions",
                column: "debt_record_id",
                principalTable: "fin_debt_records",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
