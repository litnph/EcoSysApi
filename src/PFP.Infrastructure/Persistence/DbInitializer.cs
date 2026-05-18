using Microsoft.EntityFrameworkCore;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// Idempotent database seed for i18n primitives (locales, fallback chains, UI strings) and
/// <see cref="AppDbContext.OnModelCreating"/> <c>HasData</c> support for reproducible migrations.
/// </summary>
public static class DbInitializer
{
    /// <summary>Shared UTC timestamp for all seed rows so EF migrations stay stable.</summary>
    public static readonly DateTime SeedTimestampUtc = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Guid LocaleViId = new("11111111-0000-0000-0000-000000000001");
    private static readonly Guid LocaleEnId = new("11111111-0000-0000-0000-000000000002");
    private static readonly Guid LocaleJaId = new("11111111-0000-0000-0000-000000000003");

    /// <summary>Registers static seed rows for the next EF migration (Must run before snake_case naming).</summary>
    public static void ApplyModelSeed(ModelBuilder builder)
    {
        SeedLocalesModel(builder);
        SeedTranslationFallbacksModel(builder);
        SeedUiStringsModel(builder);
    }

    /// <summary>Ensures seed data exists at runtime (safe for databases created before newer migrations).</summary>
    public static async Task EnsureAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await EnsureLocalesAsync(db, cancellationToken).ConfigureAwait(false);
        await EnsureTranslationFallbacksAsync(db, cancellationToken).ConfigureAwait(false);
        await EnsureUiStringsAsync(db, cancellationToken).ConfigureAwait(false);
    }

    private static void SeedLocalesModel(ModelBuilder builder)
    {
        builder.Entity<Locale>().HasData(
            new Locale
            {
                Id = LocaleViId,
                Code = "vi",
                Name = "Tiếng Việt",
                EnglishName = "Vietnamese",
                Direction = TextDirection.Ltr,
                IsDefault = true,
                IsActive = true,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc,
            },
            new Locale
            {
                Id = LocaleEnId,
                Code = "en",
                Name = "English",
                EnglishName = "English",
                Direction = TextDirection.Ltr,
                IsDefault = false,
                IsActive = true,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc,
            },
            new Locale
            {
                Id = LocaleJaId,
                Code = "ja",
                Name = "日本語",
                EnglishName = "Japanese",
                Direction = TextDirection.Ltr,
                IsDefault = false,
                IsActive = true,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc,
            });
    }

    private static void SeedTranslationFallbacksModel(ModelBuilder builder)
    {
        builder.Entity<TranslationFallback>().HasData(
            new TranslationFallback
            {
                Id = DeterministicGuid("translation_fallback|en|1"),
                LocaleCode = "en",
                FallbackLocaleCode = "vi",
                Priority = 1,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc,
            },
            new TranslationFallback
            {
                Id = DeterministicGuid("translation_fallback|ja|1"),
                LocaleCode = "ja",
                FallbackLocaleCode = "en",
                Priority = 1,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc,
            },
            new TranslationFallback
            {
                Id = DeterministicGuid("translation_fallback|ja|2"),
                LocaleCode = "ja",
                FallbackLocaleCode = "vi",
                Priority = 2,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc,
            });
    }

    private static void SeedUiStringsModel(ModelBuilder builder)
    {
        var defaults = UiStringSeedRows();
        var rows = new List<UIString>(defaults.Length * 2);
        foreach (var (key, vi, en, description) in defaults)
        {
            rows.Add(new UIString
            {
                Id = DeterministicGuid($"ui_string|{key}|vi"),
                Key = key,
                LocaleCode = "vi",
                Value = vi,
                Description = description,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc,
            });
            rows.Add(new UIString
            {
                Id = DeterministicGuid($"ui_string|{key}|en"),
                Key = key,
                LocaleCode = "en",
                Value = en,
                Description = description,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = SeedTimestampUtc,
            });
        }

        builder.Entity<UIString>().HasData(rows);
    }

    private static async Task EnsureLocalesAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var codes = new[] { "vi", "en", "ja" };
        var existingByCode = await db.Locales
            .Where(l => codes.Contains(l.Code))
            .ToDictionaryAsync(l => l.Code, cancellationToken)
            .ConfigureAwait(false);

        UpsertLocale(db, existingByCode, LocaleViId, "vi", "Tiếng Việt", "Vietnamese", TextDirection.Ltr, isDefault: true);
        UpsertLocale(db, existingByCode, LocaleEnId, "en", "English", "English", TextDirection.Ltr, isDefault: false);
        UpsertLocale(db, existingByCode, LocaleJaId, "ja", "日本語", "Japanese", TextDirection.Ltr, isDefault: false);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void UpsertLocale(
        AppDbContext db,
        Dictionary<string, Locale> existingByCode,
        Guid id,
        string code,
        string name,
        string englishName,
        TextDirection direction,
        bool isDefault)
    {
        if (!existingByCode.TryGetValue(code, out var existing))
        {
            db.Locales.Add(new Locale
            {
                Id = id,
                Code = code,
                Name = name,
                EnglishName = englishName,
                Direction = direction,
                IsDefault = isDefault,
                IsActive = true,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = DateTime.UtcNow,
            });
            return;
        }

        existing.Name = name;
        existing.EnglishName = englishName;
        existing.Direction = direction;
        existing.IsDefault = isDefault;
        existing.IsActive = true;
        existing.UpdatedAt = DateTime.UtcNow;
    }

    private static async Task EnsureTranslationFallbacksAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var existing = await db.TranslationFallbacks.ToListAsync(cancellationToken).ConfigureAwait(false);
        var byLocalePriority = existing.ToDictionary(f => (f.LocaleCode, f.Priority));

        UpsertFallback(db, byLocalePriority, "en", "vi", 1);
        UpsertFallback(db, byLocalePriority, "ja", "en", 1);
        UpsertFallback(db, byLocalePriority, "ja", "vi", 2);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void UpsertFallback(
        AppDbContext db,
        Dictionary<(string LocaleCode, int Priority), TranslationFallback> byLocalePriority,
        string localeCode,
        string fallbackCode,
        int priority)
    {
        if (!byLocalePriority.TryGetValue((localeCode, priority), out var row))
        {
            db.TranslationFallbacks.Add(new TranslationFallback
            {
                Id = DeterministicGuid($"translation_fallback|{localeCode}|{priority}"),
                LocaleCode = localeCode,
                FallbackLocaleCode = fallbackCode,
                Priority = priority,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = DateTime.UtcNow,
            });
            return;
        }

        row.FallbackLocaleCode = fallbackCode;
        row.UpdatedAt = DateTime.UtcNow;
    }

    private static async Task EnsureUiStringsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var seedRows = UiStringSeedRows();
        var keys = seedRows.Select(r => r.Key).Distinct().ToArray();
        var existing = await db.UIStrings
            .Where(s => keys.Contains(s.Key))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var byKeyLocale = existing.ToDictionary(s => (s.Key, s.LocaleCode));

        foreach (var (key, vi, en, description) in seedRows)
        {
            UpsertUiString(db, byKeyLocale, key, "vi", vi, description);
            UpsertUiString(db, byKeyLocale, key, "en", en, description);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void UpsertUiString(
        AppDbContext db,
        Dictionary<(string Key, string LocaleCode), UIString> byKeyLocale,
        string key,
        string localeCode,
        string value,
        string? description)
    {
        if (!byKeyLocale.TryGetValue((key, localeCode), out var row))
        {
            db.UIStrings.Add(new UIString
            {
                Id = DeterministicGuid($"ui_string|{key}|{localeCode}"),
                Key = key,
                LocaleCode = localeCode,
                Value = value,
                Description = description,
                CreatedAt = SeedTimestampUtc,
                UpdatedAt = DateTime.UtcNow,
            });
            return;
        }

        row.Value = value;
        row.Description = description;
        row.UpdatedAt = DateTime.UtcNow;
    }

    private static (string Key, string Vi, string En, string Description)[] UiStringSeedRows() =>
    [
        ("common.save", "Lưu", "Save", "Generic save button label."),
        ("common.cancel", "Hủy", "Cancel", "Generic cancel button label."),
        ("common.delete", "Xóa", "Delete", "Generic delete button label."),
        ("common.edit", "Sửa", "Edit", "Generic edit button label."),
        ("common.create", "Tạo mới", "Create", "Generic create button label."),
        ("common.confirm", "Xác nhận", "Confirm", "Generic confirmation button label."),
        ("common.loading", "Đang tải...", "Loading...", "Loading indicator label."),
        ("auth.login", "Đăng nhập", "Sign in", "Login button / page title."),
        ("auth.register", "Đăng ký", "Sign up", "Register button / page title."),
        ("auth.logout", "Đăng xuất", "Sign out", "Logout menu item."),
        ("auth.email", "Email", "Email", "Email input label."),
        ("auth.password", "Mật khẩu", "Password", "Password input label."),
        ("auth.forgot_password", "Quên mật khẩu?", "Forgot password?", "Forgot-password link on login screen."),
        ("auth.login_failed", "Email hoặc mật khẩu không đúng.", "Invalid email or password.", "Generic auth-failure error message."),
        ("auth.email_invalid", "Email không hợp lệ.", "Invalid email address.", "Inline validation message."),
        ("nav.dashboard", "Tổng quan", "Dashboard", "Main nav: dashboard."),
        ("nav.transactions", "Giao dịch", "Transactions", "Main nav: finance transactions."),
        ("nav.sources", "Nguồn tài chính", "Accounts", "Main nav: finance sources."),
        ("nav.settings", "Cài đặt", "Settings", "Main nav: settings."),
        ("login_title", "Đăng nhập", "Sign in", "Auth screen title — login."),
        ("register_title", "Đăng ký", "Create account", "Auth screen title — register."),
        ("forgot_password_title", "Quên mật khẩu", "Forgot password", "Auth screen title — forgot password."),
        ("error_invalid_credentials", "Email hoặc mật khẩu không hợp lệ.", "Invalid email or password.", "Auth error — invalid credentials."),
        ("error_account_locked", "Tài khoản đã bị khóa. Vui lòng thử lại sau.", "Your account is locked. Please try again later.", "Auth error — account locked."),
        ("success_register", "Đăng ký thành công.", "Registration successful.", "Auth success — register."),
        ("success_reset_password", "Đặt lại mật khẩu thành công.", "Your password has been reset successfully.", "Auth success — reset password."),
    ];

    private static Guid DeterministicGuid(string seed)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(seed);
#pragma warning disable CA5351 // Not used as a security primitive — deterministic id for migrations / upserts.
        var hash = System.Security.Cryptography.MD5.HashData(bytes);
#pragma warning restore CA5351
        return new Guid(hash);
    }
}
