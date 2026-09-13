namespace PFP.Application.Features.ImageImportSettings;

public static class ImageImportTypeSettingsRules
{
    public static readonly IReadOnlySet<string> SupportedTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "statement",
        "bank_transaction_list",
        "tp",
    };

    public static IReadOnlyList<ImageImportTypeSettingDto> Normalize(
        IEnumerable<ImageImportTypeSettingDto> settings) =>
        settings
            .Select(setting => new ImageImportTypeSettingDto(
                setting.Type.Trim().ToLowerInvariant(),
                setting.DisplayName.Trim(),
                setting.SourceId))
            .OrderBy(setting => setting.Type, StringComparer.Ordinal)
            .ToArray();
}
