using MediatR;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.ImageImportSettings;

public sealed record ImageImportTypeSettingDto(
    string Type,
    string DisplayName,
    Guid? SourceId);

public sealed record GetImageImportTypeSettingsQuery
    : IRequest<IReadOnlyList<ImageImportTypeSettingDto>>, IAuthorizeRequest;

public sealed record UpdateImageImportTypeSettingsCommand(
    IReadOnlyList<ImageImportTypeSettingDto> Settings)
    : IRequest<IReadOnlyList<ImageImportTypeSettingDto>>, IAuthorizeRequest
{
    public bool RequireAdmin => true;
}
