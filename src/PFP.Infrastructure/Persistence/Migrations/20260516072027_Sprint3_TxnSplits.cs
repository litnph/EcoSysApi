using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_TxnSplits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE fin_txn_splits SET status = 'settled' WHERE status IN ('paid', 'partially_paid');
                UPDATE fin_txn_splits SET status = 'pending' WHERE status IN ('cancelled', 'written_off');
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_fin_txn_splits_users_participant_user_id",
                table: "fin_txn_splits");

            migrationBuilder.DropIndex(
                name: "ix_fin_txn_splits_transaction_id",
                table: "fin_txn_splits");

            migrationBuilder.DropIndex(
                name: "ix_fin_txn_splits_participant_user_id",
                table: "fin_txn_splits");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "fin_txn_splits");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "fin_txn_splits");

            migrationBuilder.DropColumn(
                name: "note",
                table: "fin_txn_splits");

            migrationBuilder.DropColumn(
                name: "reimbursed_amount",
                table: "fin_txn_splits");

            migrationBuilder.DropColumn(
                name: "participant_user_id",
                table: "fin_txn_splits");

            migrationBuilder.RenameColumn(
                name: "participant_name",
                table: "fin_txn_splits",
                newName: "person_name");

            migrationBuilder.RenameColumn(
                name: "participant_contact",
                table: "fin_txn_splits",
                newName: "person_contact");

            migrationBuilder.RenameColumn(
                name: "share_amount",
                table: "fin_txn_splits",
                newName: "amount");

            migrationBuilder.AlterColumn<string>(
                name: "person_name",
                table: "fin_txn_splits",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "person_contact",
                table: "fin_txn_splits",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "settled_txn_id",
                table: "fin_txn_splits",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_txn_splits_settled_txn_id",
                table: "fin_txn_splits",
                column: "settled_txn_id");

            migrationBuilder.AddForeignKey(
                name: "fk_fin_txn_splits_fin_transactions_settled_txn_id",
                table: "fin_txn_splits",
                column: "settled_txn_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_txn_splits_fin_transactions_settled_txn_id",
                table: "fin_txn_splits");

            migrationBuilder.DropIndex(
                name: "ix_fin_txn_splits_settled_txn_id",
                table: "fin_txn_splits");

            migrationBuilder.DropColumn(
                name: "settled_txn_id",
                table: "fin_txn_splits");

            migrationBuilder.RenameColumn(
                name: "person_name",
                table: "fin_txn_splits",
                newName: "participant_name");

            migrationBuilder.RenameColumn(
                name: "person_contact",
                table: "fin_txn_splits",
                newName: "participant_contact");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "fin_txn_splits",
                newName: "share_amount");

            migrationBuilder.AlterColumn<string>(
                name: "participant_name",
                table: "fin_txn_splits",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "participant_contact",
                table: "fin_txn_splits",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "fin_txn_splits",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "VND");

            migrationBuilder.AddColumn<DateTime>(
                name: "due_date",
                table: "fin_txn_splits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "fin_txn_splits",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "participant_user_id",
                table: "fin_txn_splits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "reimbursed_amount",
                table: "fin_txn_splits",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_fin_txn_splits_transaction_id",
                table: "fin_txn_splits",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_txn_splits_participant_user_id",
                table: "fin_txn_splits",
                column: "participant_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_fin_txn_splits_users_participant_user_id",
                table: "fin_txn_splits",
                column: "participant_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
