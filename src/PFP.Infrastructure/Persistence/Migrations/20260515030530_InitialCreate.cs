using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log_retentions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    retain_days = table.Column<int>(type: "integer", nullable: false),
                    archive_before_delete = table.Column<bool>(type: "boolean", nullable: false),
                    archive_storage_key_prefix = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log_retentions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "locales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    english_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    direction = table.Column<string>(type: "text", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    job_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    job_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    error_message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    stack_trace = table.Column<string>(type: "text", nullable: true),
                    duration_ms = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_event_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "translation_fallbacks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locale_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fallback_locale_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    locale_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    locale_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    before_snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    after_snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_personal = table.Column<bool>(type: "boolean", nullable: false),
                    slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    default_currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uuid", nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "text", nullable: false),
                    provider_user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    linked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    storage_url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    content_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ready_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    download_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scheduled_execution_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    new_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    attempted_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_code = table.Column<string>(type: "text", nullable: false),
                    channel = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    request_ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    used_ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    timezone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    date_format = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    theme = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    device_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    left_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invited_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uuid", nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    change_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    org_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    path = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    depth = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uuid", nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    change_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    change_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    inherited = table.Column<bool>(type: "boolean", nullable: false),
                    inherited_from_space_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invited_by = table.Column<Guid>(type: "uuid", nullable: true),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    left_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uuid", nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_code = table.Column<string>(type: "text", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    settings = table.Column<string>(type: "jsonb", nullable: true),
                    enabled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    disabled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    change_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
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
                name: "fin_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    color = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    path = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    depth = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "fin_monthly_periods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    total_income = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_expense = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    net_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    category_breakdown = table.Column<string>(type: "jsonb", nullable: true),
                    source_breakdown = table.Column<string>(type: "jsonb", nullable: true),
                    transaction_count = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    credit_limit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    statement_day = table.Column<int>(type: "integer", nullable: true),
                    payment_due_day = table.Column<int>(type: "integer", nullable: true),
                    min_installment_amt = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    initial_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    icon = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    color = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    external_ref = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "fin_billing_cycles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    statement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    opening_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    closing_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_spend = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_payments = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    minimum_payment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "fin_investments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    symbol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    broker_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    avg_cost = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    current_price = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    total_invested = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_proceeds = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_dividends = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_fees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    current_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    price_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_investments", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_investments_fin_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "fin_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_fin_investments_space_modules_smodule_id",
                        column: x => x.smodule_id,
                        principalTable: "space_modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_savings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    bank_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    account_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_rate_pct = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    term_months = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    maturity_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expected_return_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    accrued_interest = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    auto_rollover = table.Column<bool>(type: "boolean", nullable: false),
                    withdrawn_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    withdrawn_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_savings", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_savings_fin_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "fin_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    change_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
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
                name: "fin_debt_record_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    change_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_debt_record_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fin_debt_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    direction = table.Column<string>(type: "text", nullable: false),
                    counterparty_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    counterparty_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    counterparty_contact = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    principal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    remaining_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    interest_rate_pct = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    origin_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    settled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uuid", nullable: true)
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
                    table.ForeignKey(
                        name: "fk_fin_debt_records_users_counterparty_user_id",
                        column: x => x.counterparty_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "fin_debt_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    debt_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    txn_type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    txn_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    remaining_after = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    installment_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_no = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    principal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    billing_cycle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    note = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_installment_pays", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_installment_pays_fin_billing_cycles_billing_cycle_id",
                        column: x => x.billing_cycle_id,
                        principalTable: "fin_billing_cycles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_installment_plan_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    change_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_installment_plan_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fin_installment_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    total_months = table.Column<int>(type: "integer", nullable: false),
                    interest_rate_pct = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    conversion_fee_rate = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    conversion_fee_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    conversion_fee_status = table.Column<string>(type: "text", nullable: false),
                    monthly_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paid_months = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uuid", nullable: true)
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smodule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    txn_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dest_source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    billing_cycle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ref_txn_id = table.Column<Guid>(type: "uuid", nullable: true),
                    debt_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    installment_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    note = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    exchange_rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    counterparty_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    external_ref = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    tags = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    last_session_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_billing_cycles_billing_cycle_id",
                        column: x => x.billing_cycle_id,
                        principalTable: "fin_billing_cycles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "fin_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_debt_records_debt_record_id",
                        column: x => x.debt_record_id,
                        principalTable: "fin_debt_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fin_transactions_fin_installment_plans_installment_plan_id",
                        column: x => x.installment_plan_id,
                        principalTable: "fin_installment_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    investment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    txn_type = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    price_per_unit = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    txn_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    realised_pn_l = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                        name: "fk_fin_investment_txns_fin_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "fin_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fin_transaction_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    change_reason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_transaction_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_transaction_history_fin_transactions_entity_id",
                        column: x => x.entity_id,
                        principalTable: "fin_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fin_txn_splits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    participant_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    participant_contact = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    share_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reimbursed_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    settled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    note = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fin_txn_splits", x => x.id);
                    table.ForeignKey(
                        name: "fk_fin_txn_splits_fin_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "fin_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_fin_txn_splits_users_participant_user_id",
                        column: x => x.participant_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "locales",
                columns: new[] { "id", "code", "created_at", "direction", "english_name", "is_active", "is_default", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ltr", "Vietnamese", true, true, "Tiếng Việt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000002"), "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ltr", "English", true, false, "English", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "ui_strings",
                columns: new[] { "id", "created_at", "description", "key", "locale_code", "updated_at", "value" },
                values: new object[,]
                {
                    { new Guid("0356fc71-8e38-3c95-168a-2ffc7869db10"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email input label.", "auth.email", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { new Guid("0ba9ad50-4b06-bd7e-7e28-b92a68b90769"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Password input label.", "auth.password", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mật khẩu" },
                    { new Guid("10ad085a-b06c-8c9f-0e4e-ce0cb85e4ffe"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Password input label.", "auth.password", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Password" },
                    { new Guid("1503aa36-eebe-5976-9800-a311a99bb7ee"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic cancel button label.", "common.cancel", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cancel" },
                    { new Guid("1574fa20-4069-667e-d17e-bc719f0c54ab"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic delete button label.", "common.delete", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete" },
                    { new Guid("20bb28e9-62f8-ad88-11d7-2df9a792fa9a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic create button label.", "common.create", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tạo mới" },
                    { new Guid("2a43e2cb-cafb-4ff0-d8da-22cf1a40a50c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Login button / page title.", "auth.login", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sign in" },
                    { new Guid("2a545327-5de8-4615-fe8e-ec59cb042637"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Inline validation message.", "auth.email_invalid", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email không hợp lệ." },
                    { new Guid("317a7446-b672-cc88-e12a-66fa6ce5b441"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Loading indicator label.", "common.loading", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Loading..." },
                    { new Guid("33bca43f-5706-3489-eca5-01eb2be265b7"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: settings.", "nav.settings", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Settings" },
                    { new Guid("4754a4c1-c1d0-dd18-ce1f-2496561ab228"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Login button / page title.", "auth.login", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng nhập" },
                    { new Guid("4e32d742-d475-c995-2ffa-4e55a2eeca8e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: dashboard.", "nav.dashboard", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dashboard" },
                    { new Guid("55ea0bda-5027-d5b0-b1cb-fc157ec4758d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic auth-failure error message.", "auth.login_failed", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email hoặc mật khẩu không đúng." },
                    { new Guid("5aeeb0c6-96ad-91d5-0224-75a52e4651e7"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic edit button label.", "common.edit", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit" },
                    { new Guid("5cba09fe-eb35-86fb-e545-f4cb59120397"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic create button label.", "common.create", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create" },
                    { new Guid("608571af-b8d6-510c-4158-ca7802deb0f8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic confirmation button label.", "common.confirm", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Xác nhận" },
                    { new Guid("673a8d2f-2878-10f3-c063-2375741e320c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email input label.", "auth.email", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email" },
                    { new Guid("6798e3b9-4acb-bf52-86f5-396679a62644"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: finance transactions.", "nav.transactions", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Giao dịch" },
                    { new Guid("687ad484-0b9f-de4c-8ed6-5ff1d33ec0e1"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: finance transactions.", "nav.transactions", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Transactions" },
                    { new Guid("6b7aaf3b-a420-598e-9750-3eb3f1291838"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic save button label.", "common.save", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Save" },
                    { new Guid("733eb310-ae67-490b-7518-ac5a0584444d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Register button / page title.", "auth.register", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng ký" },
                    { new Guid("87de4fed-8fbb-bd83-9149-0965a329f72d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic save button label.", "common.save", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lưu" },
                    { new Guid("8ba46305-adeb-395e-13b5-0afc6e6b2bf1"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Logout menu item.", "auth.logout", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sign out" },
                    { new Guid("94339832-a6c3-ff5b-d160-603c27bf02df"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: finance sources.", "nav.sources", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguồn tài chính" },
                    { new Guid("952149b5-989f-d353-9d51-5139b4343469"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Logout menu item.", "auth.logout", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng xuất" },
                    { new Guid("ae8e5f59-08c5-a5ba-1981-ddef6e38f924"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic cancel button label.", "common.cancel", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hủy" },
                    { new Guid("b294e269-67e3-f5c9-bf6a-6680588190c6"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generic edit button label.", "common.edit", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sửa" },
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
                    { new Guid("f84bd7b8-183b-167c-d9f7-1532f86d4140"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main nav: finance sources.", "nav.sources", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Accounts" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_retentions_entity_type",
                table: "audit_log_retentions",
                column: "entity_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_type_entity_id_created_at",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user_id_created_at",
                table: "audit_logs",
                columns: new[] { "user_id", "created_at" });

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
                name: "ix_fin_debt_records_counterparty_user_id",
                table: "fin_debt_records",
                column: "counterparty_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_due_date",
                table: "fin_debt_records",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_origin_transaction_id",
                table: "fin_debt_records",
                column: "origin_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_records_smodule_id_direction_status",
                table: "fin_debt_records",
                columns: new[] { "smodule_id", "direction", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_transactions_debt_record_id_txn_date",
                table: "fin_debt_transactions",
                columns: new[] { "debt_record_id", "txn_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_debt_transactions_transaction_id",
                table: "fin_debt_transactions",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_pays_billing_cycle_id",
                table: "fin_installment_pays",
                column: "billing_cycle_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_pays_installment_plan_id_sequence_no",
                table: "fin_installment_pays",
                columns: new[] { "installment_plan_id", "sequence_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_pays_status_due_date",
                table: "fin_installment_pays",
                columns: new[] { "status", "due_date" });

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
                name: "ix_fin_installment_plans_origin_transaction_id",
                table: "fin_installment_plans",
                column: "origin_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_smodule_id_status",
                table: "fin_installment_plans",
                columns: new[] { "smodule_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_installment_plans_source_id_status",
                table: "fin_installment_plans",
                columns: new[] { "source_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_investment_txns_investment_id_txn_date",
                table: "fin_investment_txns",
                columns: new[] { "investment_id", "txn_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_investment_txns_transaction_id",
                table: "fin_investment_txns",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_investments_smodule_id_is_closed",
                table: "fin_investments",
                columns: new[] { "smodule_id", "is_closed" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_investments_smodule_id_type",
                table: "fin_investments",
                columns: new[] { "smodule_id", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_investments_source_id",
                table: "fin_investments",
                column: "source_id");

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
                name: "ix_fin_savings_maturity_date",
                table: "fin_savings",
                column: "maturity_date");

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
                name: "ix_fin_transaction_history_entity_id_created_at",
                table: "fin_transaction_history",
                columns: new[] { "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_transaction_history_entity_id_version",
                table: "fin_transaction_history",
                columns: new[] { "entity_id", "version" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_billing_cycle_id",
                table: "fin_transactions",
                column: "billing_cycle_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_category_id",
                table: "fin_transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_debt_record_id",
                table: "fin_transactions",
                column: "debt_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_dest_source_id",
                table: "fin_transactions",
                column: "dest_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_transactions_installment_plan_id",
                table: "fin_transactions",
                column: "installment_plan_id");

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
                name: "ix_fin_transactions_source_id_txn_date",
                table: "fin_transactions",
                columns: new[] { "source_id", "txn_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fin_txn_splits_participant_user_id",
                table: "fin_txn_splits",
                column: "participant_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_fin_txn_splits_transaction_id",
                table: "fin_txn_splits",
                column: "transaction_id");

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
                filter: "is_default = true");

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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_auth_providers_user_id",
                table: "user_auth_providers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_avatar_uploads_user_id_active",
                table: "user_avatar_uploads",
                column: "user_id",
                unique: true,
                filter: "is_active = true");

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
                name: "ix_user_notification_prefs_user_id_module_code_channel_event_t~",
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
                name: "fk_fin_debt_records_fin_transactions_origin_transaction_id",
                table: "fin_debt_records",
                column: "origin_transaction_id",
                principalTable: "fin_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_fin_debt_transactions_fin_transactions_transaction_id",
                table: "fin_debt_transactions",
                column: "transaction_id",
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
                name: "fk_fin_debt_records_users_counterparty_user_id",
                table: "fin_debt_records");

            migrationBuilder.DropForeignKey(
                name: "fk_organizations_users_owner_id",
                table: "organizations");

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
                name: "fk_fin_billing_cycles_space_modules_smodule_id",
                table: "fin_billing_cycles");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_categories_space_modules_smodule_id",
                table: "fin_categories");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_debt_records_space_modules_smodule_id",
                table: "fin_debt_records");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_installment_plans_space_modules_smodule_id",
                table: "fin_installment_plans");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_space_modules_smodule_id",
                table: "fin_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_transactions_fin_debt_records_debt_record_id",
                table: "fin_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_fin_installment_plans_fin_transactions_origin_transaction_id",
                table: "fin_installment_plans");

            migrationBuilder.DropTable(
                name: "audit_log_retentions");

            migrationBuilder.DropTable(
                name: "audit_logs");

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
                name: "fin_monthly_periods");

            migrationBuilder.DropTable(
                name: "fin_savings");

            migrationBuilder.DropTable(
                name: "fin_source_history");

            migrationBuilder.DropTable(
                name: "fin_transaction_history");

            migrationBuilder.DropTable(
                name: "fin_txn_splits");

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
                name: "fin_sources");

            migrationBuilder.DropTable(
                name: "space_modules");

            migrationBuilder.DropTable(
                name: "spaces");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "fin_debt_records");

            migrationBuilder.DropTable(
                name: "fin_transactions");

            migrationBuilder.DropTable(
                name: "fin_billing_cycles");

            migrationBuilder.DropTable(
                name: "fin_categories");

            migrationBuilder.DropTable(
                name: "fin_installment_plans");
        }
    }
}
