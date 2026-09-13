using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.ImageImportSettings;

namespace PFP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/settings/image-import-types")]
public sealed class ImageImportSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImageImportSettingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ImageImportTypeSettingDto>>>> Get(
        CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<ImageImportTypeSettingDto>>
        {
            Data = await _mediator.Send(
                new GetImageImportTypeSettingsQuery(),
                cancellationToken),
        });

    [HttpPut]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ImageImportTypeSettingDto>>>> Update(
        [FromBody] UpdateImageImportTypeSettingsBody body,
        CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<ImageImportTypeSettingDto>>
        {
            Data = await _mediator.Send(
                new UpdateImageImportTypeSettingsCommand(body.Settings),
                cancellationToken),
        });
}

public sealed record UpdateImageImportTypeSettingsBody(
    IReadOnlyList<ImageImportTypeSettingDto> Settings);
