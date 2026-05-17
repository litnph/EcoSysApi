namespace PFP.API.Models;

/// <summary>JSON body for translation updates.</summary>
public sealed class UpdateTranslationBody
{
    /// <summary>New translated text.</summary>
    public string Value { get; set; } = string.Empty;
}
