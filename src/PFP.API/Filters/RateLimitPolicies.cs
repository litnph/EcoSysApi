using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace PFP.API.Filters;

/// <summary>
/// Centralised registration of ASP.NET Core rate-limiting policies (spec §6.1).
/// <para>
/// Policy <see cref="AuthLogin"/> caps the <c>/api/v1/auth/login</c> endpoint at
/// <b>5 requests per 15 minutes per remote IP</b>, returning HTTP 429 with a
/// JSON envelope identical to the rest of the API on overflow.
/// </para>
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>Policy name used by <c>[EnableRateLimiting("auth-login")]</c>.</summary>
    public const string AuthLogin = "auth-login";

    /// <summary>Adds platform rate-limiting policies to the service collection.</summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddPlatformRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
                var payload = """
                {"success":false,"data":null,"error":{"code":"rate_limited","messages":["Too many login attempts. Try again later."]},"meta":null}
                """;
                await context.HttpContext.Response
                    .WriteAsync(payload, cancellationToken)
                    .ConfigureAwait(false);
            };

            options.AddPolicy(AuthLogin, httpContext =>
            {
                // Partition by the real caller IP — fallback to ConnectionInfo when X-Forwarded-For is absent.
                var key = ResolveClientIp(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(15),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    });
            });
        });

        return services;
    }

    private static string ResolveClientIp(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var first = forwarded.ToString().Split(',', 2)[0].Trim();
            if (!string.IsNullOrWhiteSpace(first))
                return first;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
