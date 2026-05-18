namespace PFP.Infrastructure.Identity;

/// <summary>
/// Truncates HTTP-derived metadata to match EF <c>user_sessions</c>, <c>user_login_attempts</c>, and <c>audit_logs</c> column max lengths.
/// </summary>
internal static class HttpRequestMetadataTruncation
{
    internal const int MaxUserAgentLength = 512;
    internal const int MaxIpAddressLength = 64;

    internal static string? TruncateUserAgent(string? raw) =>
        string.IsNullOrEmpty(raw)
            ? null
            : raw.Length <= MaxUserAgentLength
                ? raw
                : raw[..MaxUserAgentLength];

    internal static string? TruncateIpAddress(string? raw) =>
        string.IsNullOrEmpty(raw)
            ? null
            : raw.Length <= MaxIpAddressLength
                ? raw
                : raw[..MaxIpAddressLength];
}
