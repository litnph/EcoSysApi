using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyUserSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_auth_provider");

            migrationBuilder.DropTable(
                name: "user_avatar_uploads");

            migrationBuilder.DropTable(
                name: "user_data_exports");

            migrationBuilder.DropTable(
                name: "user_deletion_requests");

            migrationBuilder.DropTable(
                name: "user_email_verification");

            migrationBuilder.DropTable(
                name: "user_notification_prefs");

            migrationBuilder.DropTable(
                name: "user_password_reset");

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("1463c3f0-5c09-2c4f-daf0-0006ebc51970"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("6ce687eb-b902-dcee-23eb-b0a42f1c63c8"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("7d6dc58b-35b8-da76-d653-774a0b62cc7e"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("840f6bd7-bb92-9f70-0845-b5000445aaa4"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("b42592c7-f58e-914b-4afa-c4a1db36f601"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("b8b3fab8-407a-9e12-c313-08cbd2e8d7bd"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("d92223fb-0d41-247c-3a2f-7aea6f2d5f90"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("f17989ef-194b-dec8-a7d4-9d202bcf3a1a"));

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "user_profiles",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "user_profiles");

            migrationBuilder.CreateTable(
                name: "user_auth_provider",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    last_used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    linked_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    provider_email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    provider_user_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_auth_provider", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_auth_provider_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_avatar_uploads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    storage_key = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    storage_url = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_avatar_uploads", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_avatar_uploads_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_data_exports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    download_url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    error_message = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ready_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    storage_key = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_data_exports", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_data_exports_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_deletion_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    confirmation_token_expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    confirmation_token_hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    executed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    scheduled_execution_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_deletion_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_deletion_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_email_verification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    new_email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    token_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_email_verification", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_email_verification_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_notification_prefs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    channel = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false),
                    module_code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_notification_prefs", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_notification_prefs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_password_reset",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    request_ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    token_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    used_ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_password_reset", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_password_reset_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ui_strings",
                columns: new[] { "id", "created_at", "description", "key", "locale_code", "updated_at", "value" },
                values: new object[,]
                {
                    { new Guid("1463c3f0-5c09-2c4f-daf0-0006ebc51970"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — reset password.", "success_reset_password", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặt lại mật khẩu thành công." },
                    { new Guid("6ce687eb-b902-dcee-23eb-b0a42f1c63c8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — account locked.", "error_account_locked", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tài khoản đã bị khóa. Vui lòng thử lại sau." },
                    { new Guid("7d6dc58b-35b8-da76-d653-774a0b62cc7e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — forgot password.", "forgot_password_title", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quên mật khẩu" },
                    { new Guid("840f6bd7-bb92-9f70-0845-b5000445aaa4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — forgot password.", "forgot_password_title", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forgot password" },
                    { new Guid("b42592c7-f58e-914b-4afa-c4a1db36f601"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — reset password.", "success_reset_password", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Your password has been reset successfully." },
                    { new Guid("b8b3fab8-407a-9e12-c313-08cbd2e8d7bd"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — account locked.", "error_account_locked", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Your account is locked. Please try again later." },
                    { new Guid("d92223fb-0d41-247c-3a2f-7aea6f2d5f90"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forgot-password link on login screen.", "auth.forgot_password", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quên mật khẩu?" },
                    { new Guid("f17989ef-194b-dec8-a7d4-9d202bcf3a1a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forgot-password link on login screen.", "auth.forgot_password", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forgot password?" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_auth_provider_provider_provider_user_id",
                table: "user_auth_provider",
                columns: new[] { "provider", "provider_user_id" },
                unique: true,
                filter: "[provider_user_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_auth_provider_user_id",
                table: "user_auth_provider",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_avatar_uploads_user_id_active",
                table: "user_avatar_uploads",
                column: "user_id",
                unique: true,
                filter: "[is_active] = 1");

            migrationBuilder.CreateIndex(
                name: "ix_user_avatar_uploads_user_id_created_at",
                table: "user_avatar_uploads",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_data_exports_status_created_at",
                table: "user_data_exports",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_data_exports_user_id_status",
                table: "user_data_exports",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_user_deletion_requests_confirmation_token_hash",
                table: "user_deletion_requests",
                column: "confirmation_token_hash");

            migrationBuilder.CreateIndex(
                name: "ix_user_deletion_requests_status_scheduled_execution_at",
                table: "user_deletion_requests",
                columns: new[] { "status", "scheduled_execution_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_deletion_requests_user_id_status",
                table: "user_deletion_requests",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_user_email_verification_expires_at",
                table: "user_email_verification",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_email_verification_token_hash",
                table: "user_email_verification",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_email_verification_user_id_type",
                table: "user_email_verification",
                columns: new[] { "user_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_user_notification_prefs_user_id_module_code_channel_event_type",
                table: "user_notification_prefs",
                columns: new[] { "user_id", "module_code", "channel", "event_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_password_reset_expires_at",
                table: "user_password_reset",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_password_reset_token_hash",
                table: "user_password_reset",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_password_reset_user_id",
                table: "user_password_reset",
                column: "user_id");
        }
    }
}
