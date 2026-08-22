using Microsoft.AspNetCore.HttpOverrides;

namespace PFP.API.Configuration;

/// <summary>Production hosting helpers (Render, reverse proxies).</summary>
public static class HostingConfigurationExtensions
{
    /// <summary>
    /// Maps Render/Heroku/Neon-style <c>DATABASE_URL</c> to <c>ConnectionStrings:Default</c> when not already set.
    /// </summary>
    public static WebApplicationBuilder AddRenderDatabaseUrl(this WebApplicationBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Default")))
            return builder;

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            builder.Configuration["ConnectionStrings:Default"] = ToNpgsqlConnectionString(databaseUrl);

        return builder;
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
