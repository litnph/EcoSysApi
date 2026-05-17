using System.Net.Http.Headers;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace PFP.Infrastructure.Hangfire;

/// <summary>
/// HTTP Basic auth for the Hangfire dashboard using
/// <c>HANGFIRE_DASHBOARD_USER</c> / <c>HANGFIRE_DASHBOARD_PASSWORD</c> environment variables.
/// </summary>
public sealed class BasicAuthAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <inheritdoc/>
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        var expectedUser = Environment.GetEnvironmentVariable("HANGFIRE_DASHBOARD_USER");
        var expectedPassword = Environment.GetEnvironmentVariable("HANGFIRE_DASHBOARD_PASSWORD");
        if (string.IsNullOrWhiteSpace(expectedUser) || expectedPassword is null)
            return false;

        var header = http.Request.Headers.Authorization.ToString();
        if (!AuthenticationHeaderValue.TryParse(header, out var parsed) ||
            !string.Equals(parsed.Scheme, "Basic", StringComparison.OrdinalIgnoreCase) ||
            parsed.Parameter is null)
        {
            Challenge(http);
            return false;
        }

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(parsed.Parameter));
        }
        catch (FormatException)
        {
            Challenge(http);
            return false;
        }

        var colon = decoded.IndexOf(':');
        if (colon < 0)
        {
            Challenge(http);
            return false;
        }

        var login = decoded[..colon];
        var password = decoded[(colon + 1)..];
        var ok = string.Equals(login, expectedUser, StringComparison.Ordinal) &&
                 string.Equals(password, expectedPassword, StringComparison.Ordinal);
        if (!ok)
            Challenge(http);

        return ok;
    }

    private static void Challenge(HttpContext http)
    {
        http.Response.StatusCode = StatusCodes.Status401Unauthorized;
        http.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire\"");
    }
}
