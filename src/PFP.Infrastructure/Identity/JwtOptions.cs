namespace PFP.Infrastructure.Identity;

/// <summary>Binding target for <c>Jwt</c> configuration section.</summary>
public sealed class JwtOptions
{
    /// <summary>Configuration section name (<c>Jwt</c>).</summary>
    public const string SectionName = "Jwt";

    /// <summary>Symmetric signing key — must be ≥ 256 bits.</summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>JWT <c>iss</c> claim.</summary>
    public string Issuer { get; set; } = "pfp-api";

    /// <summary>JWT <c>aud</c> claim.</summary>
    public string Audience { get; set; } = "pfp-client";
}
