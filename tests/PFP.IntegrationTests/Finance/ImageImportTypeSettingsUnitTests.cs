using PFP.Application.Features.ImageImportSettings;
using Xunit;

namespace PFP.IntegrationTests.Finance;

public sealed class ImageImportTypeSettingsUnitTests
{
    [Fact]
    public void Normalize_trims_names_and_type_keys()
    {
        var sourceId = Guid.NewGuid();
        var settings = ImageImportTypeSettingsRules.Normalize(new[]
        {
            new ImageImportTypeSettingDto(" TP ", "  TP dùng chung  ", sourceId),
            new ImageImportTypeSettingDto("statement", "Sao kê", null),
        });

        Assert.Contains(settings, setting =>
            setting.Type == "tp"
            && setting.DisplayName == "TP dùng chung"
            && setting.SourceId == sourceId);
        Assert.Contains(settings, setting =>
            setting.Type == "statement"
            && setting.SourceId is null);
    }

    [Fact]
    public void Validator_accepts_the_complete_supported_type_set()
    {
        var command = new UpdateImageImportTypeSettingsCommand(new[]
        {
            new ImageImportTypeSettingDto("statement", "Sao kê", null),
            new ImageImportTypeSettingDto("bank_transaction_list", "Danh sách giao dịch", null),
            new ImageImportTypeSettingDto("tp", "TP", null),
        });

        Assert.True(new UpdateImageImportTypeSettingsCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Validator_rejects_duplicate_or_unknown_image_types()
    {
        var command = new UpdateImageImportTypeSettingsCommand(new[]
        {
            new ImageImportTypeSettingDto("tp", "TP", null),
            new ImageImportTypeSettingDto("TP", "TP duplicate", null),
            new ImageImportTypeSettingDto("unknown", "Unknown", null),
        });

        var result = new UpdateImageImportTypeSettingsCommandValidator().Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.ErrorMessage == "Image import type settings must not contain duplicate types.");
        Assert.Contains(result.Errors, error =>
            error.ErrorMessage == "Image import type is not supported.");
    }
}
