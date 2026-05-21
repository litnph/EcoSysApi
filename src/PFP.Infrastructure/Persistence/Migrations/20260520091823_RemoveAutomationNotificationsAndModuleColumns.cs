using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAutomationNotificationsAndModuleColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "automation_logs");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "feature_flag_overrides");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "automation_rules");

            migrationBuilder.DropTable(
                name: "feature_flags");

            migrationBuilder.DropColumn(
                name: "active_org_id",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "module_code",
                table: "file_attachments");

            migrationBuilder.DropColumn(
                name: "module_code",
                table: "entity_tags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "active_org_id",
                table: "user_sessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "module_code",
                table: "file_attachments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "module_code",
                table: "entity_tags",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "automation_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    actions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    conditions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    last_run_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_run_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    trigger_type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    trigger_value = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_automation_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_automation_rules_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    author_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    edited_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    is_edited = table.Column<bool>(type: "bit", nullable: false),
                    module_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_comments_comments_parent_id",
                        column: x => x.parent_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comments_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "feature_flags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_archived = table.Column<bool>(type: "bit", nullable: false),
                    is_enabled_global = table.Column<bool>(type: "bit", nullable: false),
                    key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    rollout_percentage = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feature_flags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    body = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "automation_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    actions_executed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duration_ms = table.Column<int>(type: "int", nullable: false),
                    error_message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    triggered_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_automation_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_automation_logs_automation_rules_rule_id",
                        column: x => x.rule_id,
                        principalTable: "automation_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "feature_flag_overrides",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    flag_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false),
                    target_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    target_type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feature_flag_overrides", x => x.id);
                    table.ForeignKey(
                        name: "fk_feature_flag_overrides_feature_flags_flag_id",
                        column: x => x.flag_id,
                        principalTable: "feature_flags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_automation_logs_rule_id_triggered_at",
                table: "automation_logs",
                columns: new[] { "rule_id", "triggered_at" });

            migrationBuilder.CreateIndex(
                name: "ix_automation_rules_created_by_user_id",
                table: "automation_rules",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_automation_rules_trigger_type_is_active",
                table: "automation_rules",
                columns: new[] { "trigger_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_comments_author_id",
                table: "comments",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_entity_type_entity_id_created_at",
                table: "comments",
                columns: new[] { "entity_type", "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_comments_parent_id",
                table: "comments",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_feature_flag_overrides_expires_at",
                table: "feature_flag_overrides",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_feature_flag_overrides_flag_id_target_type_target_id",
                table: "feature_flag_overrides",
                columns: new[] { "flag_id", "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_feature_flags_key",
                table: "feature_flags",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_is_read_created_at",
                table: "notifications",
                columns: new[] { "user_id", "is_read", "created_at" });
        }
    }
}
