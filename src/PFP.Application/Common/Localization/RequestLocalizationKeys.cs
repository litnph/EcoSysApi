namespace PFP.Application.Common.Localization;

/// <summary>Keys shared between localization middleware and <see cref="Common.Interfaces.ICurrentUserService"/>.</summary>
public static class RequestLocalizationKeys
{
    /// <summary>HTTP context item name for the resolved locale code (<c>locale</c>).</summary>
    public const string HttpContextLocaleItemKey = "locale";
}
