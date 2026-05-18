namespace PFP.API.Configuration;

/// <summary>Public SPA base URL used for OAuth redirects and email deep links.</summary>
public sealed class FrontendOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Frontend";

    /// <summary>Origin without trailing slash, e.g. <c>http://localhost:3000</c>.</summary>
    public string BaseUrl { get; init; } = "http://localhost:3000";

    /// <summary>Default locale segment prepended to SPA paths.</summary>
    public string DefaultLocale { get; init; } = "vi";
}
