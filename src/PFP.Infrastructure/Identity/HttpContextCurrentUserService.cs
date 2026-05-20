using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Localization;

namespace PFP.Infrastructure.Identity;

/// <summary>Maps JWT claims on the HTTP principal to <see cref="ICurrentUserService"/>.</summary>
public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId => ReadGuid(ClaimTypes.NameIdentifier);

    public Guid? SessionId => ReadGuid(JwtClaimNames.SessionId);

    public bool IsAuthenticated => UserId is not null && SessionId is not null;

    public string? IpAddress =>
        HttpRequestMetadataTruncation.TruncateIpAddress(
            _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString());

    public string? UserAgent =>
        HttpRequestMetadataTruncation.TruncateUserAgent(
            _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString());

    public string CurrentLocale =>
        _httpContextAccessor.HttpContext?.Items.TryGetValue(RequestLocalizationKeys.HttpContextLocaleItemKey, out var localeObj) == true
        && localeObj is string loc
        && !string.IsNullOrWhiteSpace(loc)
            ? loc
            : "vi";

    private Guid? ReadGuid(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
