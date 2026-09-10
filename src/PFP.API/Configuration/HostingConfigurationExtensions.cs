using System.Security.Cryptography;
using Microsoft.AspNetCore.HttpOverrides;

namespace PFP.API.Configuration;

/// <summary>Production hosting helpers (Render, reverse proxies).</summary>
public static class HostingConfigurationExtensions
{
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
