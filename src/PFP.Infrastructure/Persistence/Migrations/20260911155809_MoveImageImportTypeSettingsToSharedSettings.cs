using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveImageImportTypeSettingsToSharedSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "image_import_type_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    source_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_image_import_type_settings", x => x.id);
                    table.ForeignKey(
                        name: "fk_image_import_type_settings_fin_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "fin_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_image_import_type_settings_source_id",
                table: "image_import_type_settings",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_image_import_type_settings_type",
                table: "image_import_type_settings",
                column: "type",
                unique: true);

            migrationBuilder.Sql(
                """
                ;WITH parsed AS
                (
                    SELECT
                        LOWER(LTRIM(RTRIM(j.[type]))) AS [type],
                        LTRIM(RTRIM(j.display_name)) AS display_name,
                        CASE
                            WHEN EXISTS (SELECT 1 FROM fin_sources s WHERE s.id = j.source_id)
                                THEN j.source_id
                            ELSE NULL
                        END AS source_id,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY LOWER(LTRIM(RTRIM(j.[type])))
                            ORDER BY p.updated_at DESC, p.id
                        ) AS row_number
                    FROM user_profiles p
                    CROSS APPLY OPENJSON(
                        CASE
                            WHEN ISJSON(p.image_import_type_settings_json) = 1
                                THEN p.image_import_type_settings_json
                            ELSE N'[]'
                        END)
                    WITH
                    (
                        [type] nvarchar(64) '$.type',
                        display_name nvarchar(80) '$.displayName',
                        source_id uniqueidentifier '$.sourceId'
                    ) j
                    WHERE LOWER(LTRIM(RTRIM(j.[type])))
                            IN ('statement', 'bank_transaction_list', 'tp')
                      AND NULLIF(LTRIM(RTRIM(j.display_name)), '') IS NOT NULL
                )
                INSERT INTO image_import_type_settings
                    (id, [type], display_name, source_id, created_at, updated_at)
                SELECT NEWID(), [type], display_name, source_id, SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM parsed
                WHERE row_number = 1;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO image_import_type_settings
                    (id, [type], display_name, source_id, created_at, updated_at)
                SELECT NEWID(), defaults.[type], defaults.display_name, NULL,
                    SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM (VALUES
                    ('statement', N'Ảnh sao kê hoặc lịch sử thẻ'),
                    ('bank_transaction_list', N'Ảnh danh sách giao dịch ngân hàng'),
                    ('tp', N'TP')
                ) defaults([type], display_name)
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM image_import_type_settings existing
                    WHERE existing.[type] = defaults.[type]
                );
                """);

            migrationBuilder.DropColumn(
                name: "image_import_type_settings_json",
                table: "user_profiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_import_type_settings_json",
                table: "user_profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE user_profiles
                SET image_import_type_settings_json =
                (
                    SELECT
                        settings.[type] AS [type],
                        settings.display_name AS displayName,
                        settings.source_id AS sourceId
                    FROM image_import_type_settings settings
                    ORDER BY settings.[type]
                    FOR JSON PATH
                );
                """);

            migrationBuilder.DropTable(
                name: "image_import_type_settings");
        }
    }
}
