using System.Security.Cryptography;
using Microsoft.AspNetCore.HttpOverrides;

namespace PFP.API.Configuration;

/// <summary>Production hosting helpers (Render, reverse proxies).</summary>
public static class HostingConfigurationExtensions
{
    private static readonly string[] DatabaseUrlEnvironmentVariables =
    [
        "DATABASE_URL",
        "NEON_DATABASE_URL",
        "POSTGRES_URL",
        "POSTGRESQL_URL",
        "DB_CONNECTION_STRING",
    ];

    private static readonly string[] DatabaseUrlSecretFiles =
    [
        "/etc/secrets/DATABASE_URL",
        "/etc/secrets/database_url",
        "/etc/secrets/ConnectionStrings__Default",
    ];

    /// <summary>
    /// Maps conventional single-underscore Render environment variables to the
    /// hierarchical ASP.NET Core configuration keys used by the application.
    /// Native keys such as <c>Jwt__Secret</c> continue to take precedence.
    /// </summary>
    public static WebApplicationBuilder AddRenderEnvironmentAliases(this WebApplicationBuilder builder)
    {
        CopyEnvironmentValueWhenMissing(builder, "Jwt:Secret", "JWT_SECRET");
        CopyEnvironmentValueWhenMissing(builder, "Jwt:Issuer", "JWT_ISSUER");
        CopyEnvironmentValueWhenMissing(builder, "Jwt:Audience", "JWT_AUDIENCE");
        return builder;
    }

    /// <summary>
    /// Keeps an existing manually-created Render service bootable when no JWT
    /// secret has been configured. The generated key is cryptographically random
    /// but instance-local, so an explicit environment secret remains preferred.
    /// </summary>
    public static WebApplicationBuilder AddRenderJwtFallback(this WebApplicationBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Secret"])
            || !string.Equals(
                Environment.GetEnvironmentVariable("RENDER"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            return builder;
        }

        builder.Configuration["Jwt:Secret"] = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
        builder.Configuration["Jwt:UsesEphemeralSecret"] = bool.TrueString;
        return builder;
    }

    /// <summary>
    /// Resolves a PostgreSQL connection from ASP.NET configuration, common
    /// Render/Neon environment aliases, or a Render runtime secret file.
    /// </summary>
    public static WebApplicationBuilder AddRenderDatabaseUrl(this WebApplicationBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Default")))
        {
            builder.Configuration["Database:ConfigurationSource"] = "connection_strings_default";
            return builder;
        }

        foreach (var variable in DatabaseUrlEnvironmentVariables)
        {
            var databaseUrl = NormalizeDatabaseUrl(
                Environment.GetEnvironmentVariable(variable));
            if (string.IsNullOrWhiteSpace(databaseUrl))
                continue;

            builder.Configuration["ConnectionStrings:Default"] = ToNpgsqlConnectionString(databaseUrl);
            builder.Configuration["Database:ConfigurationSource"] = $"environment:{variable}";
            return builder;
        }

        foreach (var path in DatabaseUrlSecretFiles)
        {
            if (!File.Exists(path))
                continue;

            try
            {
                var databaseUrl = NormalizeDatabaseUrl(File.ReadAllText(path));
                if (string.IsNullOrWhiteSpace(databaseUrl))
                    continue;

                builder.Configuration["ConnectionStrings:Default"] = ToNpgsqlConnectionString(databaseUrl);
                builder.Configuration["Database:ConfigurationSource"] = $"secret_file:{Path.GetFileName(path)}";
                return builder;
            }
            catch (IOException)
            {
                // A later startup diagnostic reports the missing configuration.
            }
            catch (UnauthorizedAccessException)
            {
                // A later startup diagnostic reports the missing configuration.
            }
        }

        builder.Configuration["Database:ConfigurationSource"] = "missing";

        return builder;
    }

    private static string? NormalizeDatabaseUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        var equalsIndex = normalized.IndexOf('=');
        if (equalsIndex > 0
            && normalized[..equalsIndex].Trim().Equals(
                "DATABASE_URL",
                StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[(equalsIndex + 1)..].Trim();
        }

        if (normalized.Length >= 2
            && ((normalized[0] == '"' && normalized[^1] == '"')
                || (normalized[0] == '\'' && normalized[^1] == '\'')))
        {
            normalized = normalized[1..^1].Trim();
        }

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    internal static string ToNpgsqlConnectionString(string databaseUrl)
    {
        if (!databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return databaseUrl;

        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');
        var sslMode = uri.Query.Contains("sslmode=require", StringComparison.OrdinalIgnoreCase)
            ? "Require"
            : "Prefer";

        return
            $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={database};Username={username};Password={password};SSL Mode={sslMode};Trust Server Certificate=true";
    }

    private static void CopyEnvironmentValueWhenMissing(
        WebApplicationBuilder builder,
        string configurationKey,
        string environmentVariable)
    {
        if (!string.IsNullOrWhiteSpace(builder.Configuration[configurationKey]))
            return;

        var value = Environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrWhiteSpace(value))
            builder.Configuration[configurationKey] = value;
    }

    /// <summary>Trust X-Forwarded-* from the edge proxy (HTTPS, client IP).</summary>
    public static WebApplication UseProductionProxy(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            return app;

        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1,
        };

        // Render's proxy addresses are dynamic and cannot be allow-listed here.
        // The service container is only reachable through Render's edge proxy.
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
        app.UseForwardedHeaders(options);

        return app;
    }
}
