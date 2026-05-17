using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PFP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4_I18n : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "locales",
                columns: new[] { "id", "code", "created_at", "direction", "english_name", "is_active", "is_default", "name", "updated_at" },
                values: new object[] { new Guid("11111111-0000-0000-0000-000000000003"), "ja", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ltr", "Japanese", true, false, "日本語", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "translation_fallbacks",
                columns: new[] { "id", "created_at", "fallback_locale_code", "locale_code", "priority", "updated_at" },
                values: new object[] { new Guid("c5acf7c5-11e8-a9bb-33be-4883d8b6a336"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "vi", "en", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "ui_strings",
                columns: new[] { "id", "created_at", "description", "key", "locale_code", "updated_at", "value" },
                values: new object[,]
                {
                    { new Guid("1463c3f0-5c09-2c4f-daf0-0006ebc51970"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — reset password.", "success_reset_password", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặt lại mật khẩu thành công." },
                    { new Guid("1aeba122-3bec-b8d7-6696-9d5970397596"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — register.", "register_title", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create account" },
                    { new Guid("262e5c4a-53ad-f20f-ecbd-d6b0cc41363c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — register.", "success_register", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng ký thành công." },
                    { new Guid("4fec625c-1cc3-467b-3369-c323d2dd83a8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — login.", "login_title", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng nhập" },
                    { new Guid("54e8110d-6833-3705-132d-386c41365538"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — register.", "register_title", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đăng ký" },
                    { new Guid("5d1bac31-66f4-c799-a59e-7e4426e9131e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — login.", "login_title", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sign in" },
                    { new Guid("6ce687eb-b902-dcee-23eb-b0a42f1c63c8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — account locked.", "error_account_locked", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tài khoản đã bị khóa. Vui lòng thử lại sau." },
                    { new Guid("73517d62-2db4-e60d-8f9f-f9b02db64cd8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — invalid credentials.", "error_invalid_credentials", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Email hoặc mật khẩu không hợp lệ." },
                    { new Guid("7d6dc58b-35b8-da76-d653-774a0b62cc7e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — forgot password.", "forgot_password_title", "vi", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quên mật khẩu" },
                    { new Guid("840f6bd7-bb92-9f70-0845-b5000445aaa4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth screen title — forgot password.", "forgot_password_title", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forgot password" },
                    { new Guid("a94e110c-7955-ba42-8d85-008540c9cac7"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — invalid credentials.", "error_invalid_credentials", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Invalid email or password." },
                    { new Guid("b42592c7-f58e-914b-4afa-c4a1db36f601"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — reset password.", "success_reset_password", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Your password has been reset successfully." },
                    { new Guid("b8b3fab8-407a-9e12-c313-08cbd2e8d7bd"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth error — account locked.", "error_account_locked", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Your account is locked. Please try again later." },
                    { new Guid("f774797b-1d7d-6507-2760-3b2cb3abd47b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Auth success — register.", "success_register", "en", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Registration successful." }
                });

            migrationBuilder.InsertData(
                table: "translation_fallbacks",
                columns: new[] { "id", "created_at", "fallback_locale_code", "locale_code", "priority", "updated_at" },
                values: new object[,]
                {
                    { new Guid("0a126a17-8e0c-355a-771a-9bd6c891eecb"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "vi", "ja", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("dbb4a633-459b-5af8-678c-b4b051323baa"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "en", "ja", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "translation_fallbacks",
                keyColumn: "id",
                keyValue: new Guid("0a126a17-8e0c-355a-771a-9bd6c891eecb"));

            migrationBuilder.DeleteData(
                table: "translation_fallbacks",
                keyColumn: "id",
                keyValue: new Guid("c5acf7c5-11e8-a9bb-33be-4883d8b6a336"));

            migrationBuilder.DeleteData(
                table: "translation_fallbacks",
                keyColumn: "id",
                keyValue: new Guid("dbb4a633-459b-5af8-678c-b4b051323baa"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("1463c3f0-5c09-2c4f-daf0-0006ebc51970"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("1aeba122-3bec-b8d7-6696-9d5970397596"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("262e5c4a-53ad-f20f-ecbd-d6b0cc41363c"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("4fec625c-1cc3-467b-3369-c323d2dd83a8"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("54e8110d-6833-3705-132d-386c41365538"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("5d1bac31-66f4-c799-a59e-7e4426e9131e"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("6ce687eb-b902-dcee-23eb-b0a42f1c63c8"));

            migrationBuilder.DeleteData(
                table: "ui_strings",
                keyColumn: "id",
                keyValue: new Guid("73517d62-2db4-e60d-8f9f-f9b02db64cd8"));

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
                keyValue: new Guid("a94e110c-7955-ba42-8d85-008540c9cac7"));

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
                keyValue: new Guid("f774797b-1d7d-6507-2760-3b2cb3abd47b"));

            migrationBuilder.DeleteData(
                table: "locales",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"));
        }
    }
}
