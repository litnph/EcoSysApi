using Microsoft.AspNetCore.Http;
using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.Identity;

/// <summary>Resolves client IP and User-Agent from the active HTTP context.</summary>
public sealed class HttpClientRequestContext : IClientRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Creates the context reader.</summary>
    public HttpClientRequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    /// <inheritdoc/>
    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
