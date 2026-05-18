namespace PFP.API.Configuration;

/// <summary>CORS for browser clients (e.g. Next.js on another origin).</summary>
public static class CorsConfigurationExtensions
{
    private const string PolicyName = "Frontend";

    private static bool HasOrigins(IConfiguration configuration) =>
        configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
            .Any(o => !string.IsNullOrWhiteSpace(o)) == true;

    /// <summary>Registers CORS when <c>Cors:AllowedOrigins</c> has at least one origin.</summary>
    public static WebApplicationBuilder AddFrontendCors(this WebApplicationBuilder builder)
    {
        if (!HasOrigins(builder.Configuration))
            return builder;

        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return builder;
    }

    /// <summary>Applies the frontend CORS policy when configured.</summary>
    public static WebApplication UseFrontendCors(this WebApplication app)
    {
        if (HasOrigins(app.Configuration))
            app.UseCors(PolicyName);

        return app;
    }
}
