using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log_retentions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    retain_days = table.Column<int>(type: "int", nullable: false),
                    archive_before_delete = table.Column<bool>(type: "bit", nullable: false),
                    archive_storage_key_prefix = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log_retentions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feature_flags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_enabled_global = table.Column<bool>(type: "bit", nullable: false),
                    rollout_percentage = table.Column<int>(type: "int", nullable: false),
                    is_archived = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feature_flags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "locales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    english_name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    direction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_default = table.Column<bool>(type: "bit", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locales", x => x.id);
                    table.UniqueConstraint("ak_locales_code", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "system_event_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    job_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    job_id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    error_message = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    stack_trace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    duration_ms = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_event_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    full_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_email_verified = table.Column<bool>(type: "bit", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feature_flag_overrides",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    flag_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    target_type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    target_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "translation_fallbacks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    locale_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fallback_locale_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    priority = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_translation_fallbacks", x => x.id);
                    table.ForeignKey(
                        name: "fk_translation_fallbacks_locales_fallback_locale_code",
                        column: x => x.fallback_locale_code,
                        principalTable: "locales",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_translation_fallbacks_locales_locale_code",
                        column: x => x.locale_code,
                        principalTable: "locales",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    field = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    locale_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_translations_locales_locale_code",
                        column: x => x.locale_code,
                        principalTable: "locales",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ui_strings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    key = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    locale_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ui_strings", x => x.id);
                    table.ForeignKey(
                        name: "fk_ui_strings_locales_locale_code",
                        column: x => x.locale_code,
                        principalTable: "locales",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    entity_type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    before_snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    after_snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    changed_fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_logs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    module_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    author_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_edited = table.Column<bool>(type: "bit", nullable: false),
                    edited_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                name: "file_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    module_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_public = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_attachments", x => x.id);
                    table.ForeignKey(
                        name: "fk_file_attachments_users_uploaded_by",
                        column: x => x.uploaded_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    body = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_personal = table.Column<bool>(type: "bit", nullable: false),
                    slug = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    owner_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    default_currency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_organizations_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_auth_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    provider_user_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    provider_email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    linked_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_auth_providers", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_auth_providers_users_user_id",
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
                    storage_key = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    storage_url = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    content_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ready_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    storage_key = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    download_url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    error_message = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    scheduled_execution_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    confirmation_token_hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    confirmation_token_expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    executed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                name: "user_email_verifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    new_email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_email_verifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_email_verifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_login_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    attempted_email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    is_success = table.Column<bool>(type: "bit", nullable: false),
                    failure_reason = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "user_notification_prefs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    module_code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    channel = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                name: "user_password_resets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    request_ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    used_ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_password_resets", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_password_resets_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    language_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    timezone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    date_format = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    theme = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_profiles_locales_language_code",
                        column: x => x.language_code,
                        principalTable: "locales",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_used_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    active_org_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    device_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    device_type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "org_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    org_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    joined_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    left_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    invited_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_org_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_org_members_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_org_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    changed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    change_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    changed_fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_history_organizations_entity_id",
                        column: x => x.entity_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    org_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    path = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    depth = table.Column<int>(type: "int", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spaces", x => x.id);
                    table.ForeignKey(
                        name: "fk_spaces_organizations_org_id",
                        column: x => x.org_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_spaces_spaces_parent_id",
                        column: x => x.parent_id,
                        principalTable: "spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "org_member_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    changed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    change_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    changed_fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_org_member_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_org_member_history_org_members_entity_id",
                        column: x => x.entity_id,
                        principalTable: "org_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "space_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    changed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    change_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    changed_fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_space_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_space_history_spaces_entity_id",
                        column: x => x.entity_id,
                        principalTable: "spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "space_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    space_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    inherited = table.Column<bool>(type: "bit", nullable: false),
                    inherited_from_space_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    invited_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    joined_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    left_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_space_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_space_members_spaces_inherited_from_space_id",
                        column: x => x.inherited_from_space_id,
                        principalTable: "spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_space_members_spaces_space_id",
                        column: x => x.space_id,
                        principalTable: "spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_space_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "space_modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    space_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    module_code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false),
                    settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    enabled_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    disabled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_space_modules", x => x.id);
                    table.ForeignKey(
                        name: "fk_space_modules_spaces_space_id",
                        column: x => x.space_id,
                        principalTable: "spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "space_member_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    changed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    change_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    changed_fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_space_member_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_space_member_history_space_members_entity_id",
                        column: x => x.entity_id,
                        principalTable: "space_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "automation_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    trigger_type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    trigger_value = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    conditions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    actions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    last_run_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_run_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_automation_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_automation_rules_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_automation_rules_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    kind = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    icon = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    color = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    parent_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    path = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    depth = table.Column<int>(type: "int", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_default = table.Column<bool>(type: "bit", nullable: false),
                    is_system = table.Column<bool>(type: "bit", nullable: false),
                    description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_categories_fin_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "fin_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_categories_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_investments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    current_value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total_invested = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total_returned = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_investments", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_investments_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_monthly_periods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    month = table.Column<int>(type: "int", nullable: false),
                    total_income = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total_expense = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    net = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    closed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    closed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    category_breakdown = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    source_breakdown = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_monthly_periods", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_monthly_periods_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_monthly_periods_users_closed_by",
                        column: x => x.closed_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "fin_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    credit_limit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    statement_day = table.Column<int>(type: "int", nullable: true),
                    payment_due_day = table.Column<int>(type: "int", nullable: true),
                    min_installment_amt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    initial_balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    icon = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    color = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_archived = table.Column<bool>(type: "bit", nullable: false),
                    external_ref = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_sources", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_sources_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    usage_count = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_tags_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "automation_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    triggered_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    actions_executed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    error_message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    duration_ms = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                name: "fin_billing_cycles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    source_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    statement_date = table.Column<DateOnly>(type: "date", nullable: false),
                    payment_due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    paid_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    closed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_billing_cycles", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_billing_cycles_fin_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "fin_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_billing_cycles_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_savings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    source_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    target_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    current_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_rate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    maturity_date = table.Column<DateOnly>(type: "date", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_savings", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_savings_fin_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "fin_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_savings_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_source_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    changed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    change_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    changed_fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_source_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_source_history_fin_sources_entity_id",
                        column: x => x.entity_id,
                        principalTable: "fin_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entity_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tag_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    module_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tagged_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entity_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_entity_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entity_tags_users_tagged_by",
                        column: x => x.tagged_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_debt_record_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    changed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    change_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    changed_fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_debt_record_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fin_debt_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    direction = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    person_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    person_contact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    original_txn_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    original_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    remaining_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_debt_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_debt_records_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_debt_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    debt_record_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    txn_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    txn_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_debt_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_debt_transactions_fin_debt_records_debt_record_id",
                        column: x => x.debt_record_id,
                        principalTable: "fin_debt_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fin_installment_pays",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    installment_plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    installment_number = table.Column<int>(type: "int", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    paid_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_installment_pays", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fin_installment_plan_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    changed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    change_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    changed_fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_installment_plan_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fin_installment_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    origin_transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    source_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total_months = table.Column<int>(type: "int", nullable: false),
                    monthly_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_rate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    conversion_fee_rate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    conversion_fee_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    conversion_fee_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    conversion_fee_txn_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    cancellation_reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_installment_plans", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_installment_plans_fin_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "fin_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_installment_plans_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    txn_date = table.Column<DateOnly>(type: "date", nullable: false),
                    source_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dest_source_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    billing_cycle_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    monthly_period_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ref_txn_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    installment_plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    exchange_rate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    counterparty_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    external_ref = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    tags = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    updated_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                        column: x => x.billing_cycle_id,
                        principalTable: "fin_billing_cycles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "fin_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_installment_plans_installment_plan_id",
                        column: x => x.installment_plan_id,
                        principalTable: "fin_installment_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_monthly_periods_monthly_period_id",
                        column: x => x.monthly_period_id,
                        principalTable: "fin_monthly_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_sources_dest_source_id",
                        column: x => x.dest_source_id,
                        principalTable: "fin_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "fin_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_transactions_ref_txn_id",
                        column: x => x.ref_txn_id,
                        principalTable: "fin_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_transactions_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_investment_txns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    investment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    txn_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    price_per_unit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    txn_date = table.Column<DateOnly>(type: "date", nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    linked_txn_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_investment_txns", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_investment_txns_fin_investments_investment_id",
                        column: x => x.investment_id,
                        principalTable: "fin_investments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_fin_investment_txns_fin_transactions_linked_txn_id",
                        column: x => x.linked_txn_id,
                        principalTable: "fin_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_transaction_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    changed_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    change_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    changed_fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    change_reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_transaction_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_transaction_history_fin_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "fin_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fin_txn_splits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    person_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    person_contact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    settled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    settled_txn_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_txn_splits", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_txn_splits_fin_transactions_settled_txn_id",
                        column: x => x.settled_txn_id,
                        principalTable: "fin_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_fin_txn_splits_fin_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "fin_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "locales",
                columns: new[] { "id", "code", "created_at", "direction", "english_name", "is_active", "is_default", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ltr", "Vietnamese", true, true, "Tiếng Việt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000002"), "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ltr", "English", true, false, "English", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000003"), "ja", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ltr", "Japanese", true, false, "日本語", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "translation_fallbacks",
                columns: new[] { "id", "created_at", "fallback_locale_code", "locale_code", "priority", "updated_at" },
                values: new object[,]
                {
                    { new Guid("0a126a17-8e0c-355a-771a-9bd6c891eecb"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "vi", "ja", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c5acf7c5-11e8-a9bb-33be-4883d8b6a336"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "vi", "en", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("dbb4a633-459b-5af8-678c-b4b051323baa"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", "ja", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "ui_strings",
                columns: new[] { "id", "created_at", "description", "key", "locale_code", "updated_at", "value" },
                values: new object[,]
                {
                    { new Guid("0356fc71-8e38-3c95-168a-2ffc7869db10"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email input label.", "auth.email", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { new Guid("0ba9ad50-4b06-bd7e-7e28-b92a68b90769"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Password input label.", "auth.password", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mật khẩu" },
                    { new Guid("10ad085a-b06c-8c9f-0e4e-ce0cb85e4ffe"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Password input label.", "auth.password", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Password" },
                    { new Guid("1463c3f0-5c09-2c4f-daf0-0006ebc51970"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — reset password.", "success_reset_password", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặt lại mật khẩu thành công." },
                    { new Guid("1503aa36-eebe-5976-9800-a311a99bb7ee"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic cancel button label.", "common.cancel", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cancel" },
                    { new Guid("1574fa20-4069-667e-d17e-bc719f0c54ab"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic delete button label.", "common.delete", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete" },
                    { new Guid("1aeba122-3bec-b8d7-6696-9d5970397596"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — register.", "register_title", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create account" },
                    { new Guid("20bb28e9-62f8-ad88-11d7-2df9a792fa9a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic create button label.", "common.create", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tạo mới" },
                    { new Guid("262e5c4a-53ad-f20f-ecbd-d6b0cc41363c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — register.", "success_register", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng ký thành công." },
                    { new Guid("2a43e2cb-cafb-4ff0-d8da-22cf1a40a50c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Login button / page title.", "auth.login", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sign in" },
                    { new Guid("2a545327-5de8-4615-fe8e-ec59cb042637"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Inline validation message.", "auth.email_invalid", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email không hợp lệ." },
                    { new Guid("317a7446-b672-cc88-e12a-66fa6ce5b441"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Loading indicator label.", "common.loading", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Loading..." },
                    { new Guid("33bca43f-5706-3489-eca5-01eb2be265b7"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: settings.", "nav.settings", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Settings" },
                    { new Guid("4754a4c1-c1d0-dd18-ce1f-2496561ab228"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Login button / page title.", "auth.login", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng nhập" },
                    { new Guid("4e32d742-d475-c995-2ffa-4e55a2eeca8e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: dashboard.", "nav.dashboard", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dashboard" },
                    { new Guid("4fec625c-1cc3-467b-3369-c323d2dd83a8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — login.", "login_title", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng nhập" },
                    { new Guid("54e8110d-6833-3705-132d-386c41365538"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — register.", "register_title", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng ký" },
                    { new Guid("55ea0bda-5027-d5b0-b1cb-fc157ec4758d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic auth-failure error message.", "auth.login_failed", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email hoặc mật khẩu không đúng." },
                    { new Guid("5aeeb0c6-96ad-91d5-0224-75a52e4651e7"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic edit button label.", "common.edit", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit" },
                    { new Guid("5cba09fe-eb35-86fb-e545-f4cb59120397"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic create button label.", "common.create", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create" },
                    { new Guid("5d1bac31-66f4-c799-a59e-7e4426e9131e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — login.", "login_title", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sign in" },
                    { new Guid("608571af-b8d6-510c-4158-ca7802deb0f8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic confirmation button label.", "common.confirm", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Xác nhận" },
                    { new Guid("673a8d2f-2878-10f3-c063-2375741e320c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email input label.", "auth.email", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { new Guid("6798e3b9-4acb-bf52-86f5-396679a62644"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: finance transactions.", "nav.transactions", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Giao dịch" },
                    { new Guid("687ad484-0b9f-de4c-8ed6-5ff1d33ec0e1"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: finance transactions.", "nav.transactions", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Transactions" },
                    { new Guid("6b7aaf3b-a420-598e-9750-3eb3f1291838"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic save button label.", "common.save", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Save" },
                    { new Guid("6ce687eb-b902-dcee-23eb-b0a42f1c63c8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — account locked.", "error_account_locked", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tài khoản đã bị khóa. Vui lòng thử lại sau." },
                    { new Guid("733eb310-ae67-490b-7518-ac5a0584444d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Register button / page title.", "auth.register", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng ký" },
                    { new Guid("73517d62-2db4-e60d-8f9f-f9b02db64cd8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — invalid credentials.", "error_invalid_credentials", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email hoặc mật khẩu không hợp lệ." },
                    { new Guid("7d6dc58b-35b8-da76-d653-774a0b62cc7e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — forgot password.", "forgot_password_title", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quên mật khẩu" },
                    { new Guid("840f6bd7-bb92-9f70-0845-b5000445aaa4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — forgot password.", "forgot_password_title", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forgot password" },
                    { new Guid("87de4fed-8fbb-bd83-9149-0965a329f72d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic save button label.", "common.save", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lưu" },
                    { new Guid("8ba46305-adeb-395e-13b5-0afc6e6b2bf1"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Logout menu item.", "auth.logout", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sign out" },
                    { new Guid("94339832-a6c3-ff5b-d160-603c27bf02df"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: finance sources.", "nav.sources", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguồn tài chính" },
                    { new Guid("952149b5-989f-d353-9d51-5139b4343469"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Logout menu item.", "auth.logout", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng xuất" },
                    { new Guid("a94e110c-7955-ba42-8d85-008540c9cac7"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — invalid credentials.", "error_invalid_credentials", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Invalid email or password." },
                    { new Guid("ae8e5f59-08c5-a5ba-1981-ddef6e38f924"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic cancel button label.", "common.cancel", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hủy" },
                    { new Guid("b294e269-67e3-f5c9-bf6a-6680588190c6"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic edit button label.", "common.edit", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sửa" },
                    { new Guid("b42592c7-f58e-914b-4afa-c4a1db36f601"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — reset password.", "success_reset_password", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Your password has been reset successfully." },
                    { new Guid("b8b3fab8-407a-9e12-c313-08cbd2e8d7bd"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — account locked.", "error_account_locked", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Your account is locked. Please try again later." },
                    { new Guid("ba248463-e3e9-bbba-8f60-7be997d6b287"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic delete button label.", "common.delete", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Xóa" },
                    { new Guid("bc3f4c9c-36c5-1325-ee2b-13e58f749d5b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic auth-failure error message.", "auth.login_failed", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Invalid email or password." },
                    { new Guid("c3ae1b21-b4cb-3551-7817-f91d2b226997"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Loading indicator label.", "common.loading", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đang tải..." },
                    { new Guid("c80f0f81-9ba1-74f2-0b0f-f19920f1a85c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: dashboard.", "nav.dashboard", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tổng quan" },
                    { new Guid("d92223fb-0d41-247c-3a2f-7aea6f2d5f90"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forgot-password link on login screen.", "auth.forgot_password", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quên mật khẩu?" },
                    { new Guid("da70554a-6edb-99c2-a26c-2bde4139f81c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: settings.", "nav.settings", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cài đặt" },
                    { new Guid("f0527c9b-319a-da46-e872-40de6de3dbf7"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic confirmation button label.", "common.confirm", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Confirm" },
                    { new Guid("f17989ef-194b-dec8-a7d4-9d202bcf3a1a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forgot-password link on login screen.", "auth.forgot_password", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forgot password?" },
                    { new Guid("f1b02c77-f857-e64b-d8f0-92d2af4dde23"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Inline validation message.", "auth.email_invalid", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Invalid email address." },
                    { new Guid("f6a2e0bb-0895-fb98-20c4-d8cc41644fb4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Register button / page title.", "auth.register", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sign up" },
                    { new Guid("f774797b-1d7d-6507-2760-3b2cb3abd47b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — register.", "success_register", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Registration successful." },
                    { new Guid("f84bd7b8-183b-167c-d9f7-1532f86d4140"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: finance sources.", "nav.sources", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Accounts" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_retentions_entity_type",
                table: "audit_log_retentions",
                column: "entity_type",
                unique: true,
                filter: "[entity_type] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_type_entity_id_created_at",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user_id_created_at",
                table: "audit_logs",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_automation_logs_rule_id_triggered_at",
                table: "automation_logs",
                columns: new[] { "rule_id", "triggered_at" });

            migrationBuilder.CreateIndex(
                name: "ix_automation_rules_created_by_user_id",
                table: "automation_rules",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_automation_rules_smodule_id_is_active",
                table: "automation_rules",
                columns: new[] { "smodule_id", "is_active" });

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
                name: "ix_entity_tags_entity_type_entity_id",
                table: "entity_tags",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_entity_tags_tag_id_entity_type_entity_id",
                table: "entity_tags",
                columns: new[] { "tag_id", "entity_type", "entity_id" },
                unique: true,
                filter: "[is_deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "ix_entity_tags_tagged_by",
                table: "entity_tags",
                column: "tagged_by");

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
                name: "ix_file_attachments_entity_type_entity_id",
                table: "file_attachments",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_file_attachments_uploaded_by",
                table: "file_attachments",
                column: "uploaded_by");

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

            migrationBuilder.CreateIndex(
                name: "ix_fin_categories_parent_id",
                table: "fin_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_categories_smodule_id_code",
                table: "fin_categories",
                columns: new[] { "smodule_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_categories_smodule_id_kind",
                table: "fin_categories",
                columns: new[] { "smodule_id", "kind" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_categories_smodule_id_parent_id",
                table: "fin_categories",
                columns: new[] { "smodule_id", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_record_history_created_at",
                table: "fin_debt_record_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_record_history_entity_id_created_at",
                table: "fin_debt_record_history",
                columns: new[] { "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_record_history_entity_id_version",
                table: "fin_debt_record_history",
                columns: new[] { "entity_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_original_txn_id",
                table: "fin_debt_records",
                column: "original_txn_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_smodule_id_direction_status",
                table: "fin_debt_records",
                columns: new[] { "smodule_id", "direction", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_smodule_id_due_date",
                table: "fin_debt_records",
                columns: new[] { "smodule_id", "due_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_transactions_debt_record_id_txn_date",
                table: "fin_debt_transactions",
                columns: new[] { "debt_record_id", "txn_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_transactions_txn_id",
                table: "fin_debt_transactions",
                column: "txn_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_pays_installment_plan_id_installment_number",
                table: "fin_installment_pays",
                columns: new[] { "installment_plan_id", "installment_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_pays_installment_plan_id_status",
                table: "fin_installment_pays",
                columns: new[] { "installment_plan_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_pays_transaction_id",
                table: "fin_installment_pays",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plan_history_created_at",
                table: "fin_installment_plan_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plan_history_entity_id_created_at",
                table: "fin_installment_plan_history",
                columns: new[] { "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plan_history_entity_id_version",
                table: "fin_installment_plan_history",
                columns: new[] { "entity_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_conversion_fee_txn_id",
                table: "fin_installment_plans",
                column: "conversion_fee_txn_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_origin_transaction_id",
                table: "fin_installment_plans",
                column: "origin_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_smodule_id_status",
                table: "fin_installment_plans",
                columns: new[] { "smodule_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_source_id",
                table: "fin_installment_plans",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_investment_txns_investment_id_txn_date",
                table: "fin_investment_txns",
                columns: new[] { "investment_id", "txn_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_investment_txns_linked_txn_id",
                table: "fin_investment_txns",
                column: "linked_txn_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_investments_smodule_id_type",
                table: "fin_investments",
                columns: new[] { "smodule_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_monthly_periods_closed_by",
                table: "fin_monthly_periods",
                column: "closed_by");

            migrationBuilder.CreateIndex(
                name: "ix_fin_monthly_periods_smodule_id_status",
                table: "fin_monthly_periods",
                columns: new[] { "smodule_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_monthly_periods_smodule_id_year_month",
                table: "fin_monthly_periods",
                columns: new[] { "smodule_id", "year", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_savings_smodule_id_status",
                table: "fin_savings",
                columns: new[] { "smodule_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_savings_source_id",
                table: "fin_savings",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_source_history_created_at",
                table: "fin_source_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_fin_source_history_entity_id_created_at",
                table: "fin_source_history",
                columns: new[] { "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_source_history_entity_id_version",
                table: "fin_source_history",
                columns: new[] { "entity_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_sources_smodule_id",
                table: "fin_sources",
                column: "smodule_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_sources_smodule_id_is_archived",
                table: "fin_sources",
                columns: new[] { "smodule_id", "is_archived" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_sources_smodule_id_type",
                table: "fin_sources",
                columns: new[] { "smodule_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_transaction_history_created_at",
                table: "fin_transaction_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transaction_history_transaction_id_created_at",
                table: "fin_transaction_history",
                columns: new[] { "transaction_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_transaction_history_transaction_id_version",
                table: "fin_transaction_history",
                columns: new[] { "transaction_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_billing_cycle_id",
                table: "fin_transactions",
                column: "billing_cycle_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_category_id",
                table: "fin_transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_dest_source_id",
                table: "fin_transactions",
                column: "dest_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_installment_plan_id",
                table: "fin_transactions",
                column: "installment_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_monthly_period_id",
                table: "fin_transactions",
                column: "monthly_period_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_ref_txn_id",
                table: "fin_transactions",
                column: "ref_txn_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_smodule_id_txn_date",
                table: "fin_transactions",
                columns: new[] { "smodule_id", "txn_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_smodule_id_type_txn_date",
                table: "fin_transactions",
                columns: new[] { "smodule_id", "type", "txn_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_source_id",
                table: "fin_transactions",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_source_id_txn_date",
                table: "fin_transactions",
                columns: new[] { "source_id", "txn_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_txn_splits_settled_txn_id",
                table: "fin_txn_splits",
                column: "settled_txn_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_txn_splits_transaction_id_status",
                table: "fin_txn_splits",
                columns: new[] { "transaction_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_locales_code",
                table: "locales",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_locales_is_default_singleton",
                table: "locales",
                column: "is_default",
                unique: true,
                filter: "[is_default] = 1");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_is_read_created_at",
                table: "notifications",
                columns: new[] { "user_id", "is_read", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_org_member_history_created_at",
                table: "org_member_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_org_member_history_entity_id_created_at",
                table: "org_member_history",
                columns: new[] { "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_org_member_history_entity_id_version",
                table: "org_member_history",
                columns: new[] { "entity_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_org_members_org_id_user_id",
                table: "org_members",
                columns: new[] { "org_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_org_members_user_id_is_active",
                table: "org_members",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_organization_history_created_at",
                table: "organization_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_organization_history_entity_id_created_at",
                table: "organization_history",
                columns: new[] { "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_organization_history_entity_id_version",
                table: "organization_history",
                columns: new[] { "entity_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_organizations_owner_id",
                table: "organizations",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_organizations_slug",
                table: "organizations",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_space_history_created_at",
                table: "space_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_space_history_entity_id_created_at",
                table: "space_history",
                columns: new[] { "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_space_history_entity_id_version",
                table: "space_history",
                columns: new[] { "entity_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_space_member_history_created_at",
                table: "space_member_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_space_member_history_entity_id_created_at",
                table: "space_member_history",
                columns: new[] { "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_space_member_history_entity_id_version",
                table: "space_member_history",
                columns: new[] { "entity_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_space_members_inherited_from_space_id",
                table: "space_members",
                column: "inherited_from_space_id");

            migrationBuilder.CreateIndex(
                name: "ix_space_members_space_id_user_id",
                table: "space_members",
                columns: new[] { "space_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_space_members_user_id",
                table: "space_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_space_modules_space_id_module_code",
                table: "space_modules",
                columns: new[] { "space_id", "module_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spaces_org_id_parent_id",
                table: "spaces",
                columns: new[] { "org_id", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "ix_spaces_parent_id",
                table: "spaces",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_spaces_path",
                table: "spaces",
                column: "path");

            migrationBuilder.CreateIndex(
                name: "ix_system_event_logs_entity_type_entity_id_created_at",
                table: "system_event_logs",
                columns: new[] { "entity_type", "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_system_event_logs_event_type_created_at",
                table: "system_event_logs",
                columns: new[] { "event_type", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_system_event_logs_job_name_created_at",
                table: "system_event_logs",
                columns: new[] { "job_name", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tags_smodule_id_name",
                table: "tags",
                columns: new[] { "smodule_id", "name" },
                unique: true,
                filter: "[is_deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "ix_translation_fallbacks_fallback_locale_code",
                table: "translation_fallbacks",
                column: "fallback_locale_code");

            migrationBuilder.CreateIndex(
                name: "ix_translation_fallbacks_locale_code_priority",
                table: "translation_fallbacks",
                columns: new[] { "locale_code", "priority" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_translations_entity_type_entity_id_field_locale_code",
                table: "translations",
                columns: new[] { "entity_type", "entity_id", "field", "locale_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_translations_entity_type_entity_id_locale_code",
                table: "translations",
                columns: new[] { "entity_type", "entity_id", "locale_code" });

            migrationBuilder.CreateIndex(
                name: "ix_translations_locale_code",
                table: "translations",
                column: "locale_code");

            migrationBuilder.CreateIndex(
                name: "ix_ui_strings_key_locale_code",
                table: "ui_strings",
                columns: new[] { "key", "locale_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ui_strings_locale_code",
                table: "ui_strings",
                column: "locale_code");

            migrationBuilder.CreateIndex(
                name: "ix_user_auth_providers_provider_provider_user_id",
                table: "user_auth_providers",
                columns: new[] { "provider", "provider_user_id" },
                unique: true,
                filter: "[provider_user_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_auth_providers_user_id",
                table: "user_auth_providers",
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
                name: "ix_user_email_verifications_expires_at",
                table: "user_email_verifications",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_email_verifications_token_hash",
                table: "user_email_verifications",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_email_verifications_user_id_type",
                table: "user_email_verifications",
                columns: new[] { "user_id", "type" });

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

            migrationBuilder.CreateIndex(
                name: "ix_user_notification_prefs_user_id_module_code_channel_event_type",
                table: "user_notification_prefs",
                columns: new[] { "user_id", "module_code", "channel", "event_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_password_resets_expires_at",
                table: "user_password_resets",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_password_resets_token_hash",
                table: "user_password_resets",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_password_resets_user_id",
                table: "user_password_resets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_language_code",
                table: "user_profiles",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_user_id",
                table: "user_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_expires_at",
                table: "user_sessions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_token_hash",
                table: "user_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_id_expires_at",
                table: "user_sessions",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_debt_record_history_fin_debt_records_entity_id",
                table: "fin_debt_record_history",
                column: "entity_id",
                principalTable: "fin_debt_records",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "fk_fin_installment_pays_fin_installment_plans_installment_plan_id",
                table: "fin_installment_pays",
                column: "installment_plan_id",
                principalTable: "fin_installment_plans",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_installment_pays_fin_transactions_transaction_id",
                table: "fin_installment_pays",
                column: "transaction_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_installment_plan_history_fin_installment_plans_entity_id",
                table: "fin_installment_plan_history",
                column: "entity_id",
                principalTable: "fin_installment_plans",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_installment_plans_fin_transactions_conversion_fee_txn_id",
                table: "fin_installment_plans",
                column: "conversion_fee_txn_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_installment_plans_fin_transactions_origin_transaction_id",
                table: "fin_installment_plans",
                column: "origin_transaction_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fin_monthly_periods_users_closed_by",
                table: "fin_monthly_periods");

            migrationBuilder.DropForeignKey(
                name: "fk_organizations_users_owner_id",
                table: "organizations");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_billing_cycles_space_modules_smodule_id",
                table: "fin_billing_cycles");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_categories_space_modules_smodule_id",
                table: "fin_categories");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_installment_plans_space_modules_smodule_id",
                table: "fin_installment_plans");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_monthly_periods_space_modules_smodule_id",
                table: "fin_monthly_periods");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_sources_space_modules_smodule_id",
                table: "fin_sources");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_space_modules_smodule_id",
                table: "fin_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_billing_cycles_fin_sources_source_id",
                table: "fin_billing_cycles");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_installment_plans_fin_sources_source_id",
                table: "fin_installment_plans");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_sources_dest_source_id",
                table: "fin_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_sources_source_id",
                table: "fin_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_installment_plans_fin_transactions_conversion_fee_txn_id",
                table: "fin_installment_plans");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_installment_plans_fin_transactions_origin_transaction_id",
                table: "fin_installment_plans");

            migrationBuilder.DropTable(
                name: "audit_log_retentions");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "automation_logs");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "entity_tags");

            migrationBuilder.DropTable(
                name: "feature_flag_overrides");

            migrationBuilder.DropTable(
                name: "file_attachments");

            migrationBuilder.DropTable(
                name: "fin_debt_record_history");

            migrationBuilder.DropTable(
                name: "fin_debt_transactions");

            migrationBuilder.DropTable(
                name: "fin_installment_pays");

            migrationBuilder.DropTable(
                name: "fin_installment_plan_history");

            migrationBuilder.DropTable(
                name: "fin_investment_txns");

            migrationBuilder.DropTable(
                name: "fin_savings");

            migrationBuilder.DropTable(
                name: "fin_source_history");

            migrationBuilder.DropTable(
                name: "fin_transaction_history");

            migrationBuilder.DropTable(
                name: "fin_txn_splits");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "org_member_history");

            migrationBuilder.DropTable(
                name: "organization_history");

            migrationBuilder.DropTable(
                name: "space_history");

            migrationBuilder.DropTable(
                name: "space_member_history");

            migrationBuilder.DropTable(
                name: "system_event_logs");

            migrationBuilder.DropTable(
                name: "translation_fallbacks");

            migrationBuilder.DropTable(
                name: "translations");

            migrationBuilder.DropTable(
                name: "ui_strings");

            migrationBuilder.DropTable(
                name: "user_auth_providers");

            migrationBuilder.DropTable(
                name: "user_avatar_uploads");

            migrationBuilder.DropTable(
                name: "user_data_exports");

            migrationBuilder.DropTable(
                name: "user_deletion_requests");

            migrationBuilder.DropTable(
                name: "user_email_verifications");

            migrationBuilder.DropTable(
                name: "user_login_attempts");

            migrationBuilder.DropTable(
                name: "user_notification_prefs");

            migrationBuilder.DropTable(
                name: "user_password_resets");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "automation_rules");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "feature_flags");

            migrationBuilder.DropTable(
                name: "fin_debt_records");

            migrationBuilder.DropTable(
                name: "fin_investments");

            migrationBuilder.DropTable(
                name: "org_members");

            migrationBuilder.DropTable(
                name: "space_members");

            migrationBuilder.DropTable(
                name: "locales");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "space_modules");

            migrationBuilder.DropTable(
                name: "spaces");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "fin_sources");

            migrationBuilder.DropTable(
                name: "fin_transactions");

            migrationBuilder.DropTable(
                name: "fin_billing_cycles");

            migrationBuilder.DropTable(
                name: "fin_categories");

            migrationBuilder.DropTable(
                name: "fin_installment_plans");

            migrationBuilder.DropTable(
                name: "fin_monthly_periods");
        }
    }
}
