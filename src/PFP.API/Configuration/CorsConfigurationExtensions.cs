namespace PFP.API.Configuration;

/// <summary>CORS for browser clients (e.g. Next.js on another origin).</summary>
public static class CorsConfigurationExtensions
{
    private const string PolicyName = "Frontend";

    private static bool AllowAll(IConfiguration configuration) =>
        configuration.GetValue("Cors:AllowAll", false);

    private static bool HasOrigins(IConfiguration configuration) =>
        configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
            .Any(o => !string.IsNullOrWhiteSpace(o)) == true;

    private static bool IsCorsEnabled(IConfiguration configuration) =>
        AllowAll(configuration) || HasOrigins(configuration);

    /// <summary>Registers CORS when <c>Cors:AllowAll</c> is true or <c>Cors:AllowedOrigins</c> is set.</summary>
    public static WebApplicationBuilder AddFrontendCors(this WebApplicationBuilder builder)
    {
        if (!IsCorsEnabled(builder.Configuration))
            return builder;

        var allowAll = AllowAll(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (allowAll)
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
                else
                {
                    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .Select(o => o.Trim().TrimEnd('/'))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            });
        });

        return builder;
    }

    /// <summary>Applies the frontend CORS policy when configured.</summary>
    public static WebApplication UseFrontendCors(this WebApplication app)
    {
        if (IsCorsEnabled(app.Configuration))
            app.UseCors(PolicyName);

        return app;
    }
}
