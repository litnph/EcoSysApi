namespace PFP.Application.Common.Interfaces;

/// <summary>Best-effort HTTP client metadata forwarded into auth handlers (sessions, audit, forensics).</summary>
public interface IClientRequestContext
{
    /// <summary>Remote IP when available.</summary>
    string? IpAddress { get; }

    /// <summary>Raw User-Agent header when available.</summary>
    string? UserAgent { get; }
}
