using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserLoginAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_login_attempts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_login_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    attempted_email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    failure_reason = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    is_success = table.Column<bool>(type: "bit", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_agent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_login_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_login_attempts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_login_attempts_attempted_email_created_at",
                table: "user_login_attempts",
                columns: new[] { "attempted_email", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_login_attempts_ip_address_created_at",
                table: "user_login_attempts",
                columns: new[] { "ip_address", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_login_attempts_user_id_created_at",
                table: "user_login_attempts",
                columns: new[] { "user_id", "created_at" });
        }
    }
}
