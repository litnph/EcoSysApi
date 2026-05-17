using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Gdpr.ExportData;

namespace PFP.API.Controllers;

/// <summary>GDPR data export for the authenticated user.</summary>
[ApiController]
[Authorize]
[Route("api/v1/user")]
public sealed class UserDataController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public UserDataController(IMediator mediator) => _mediator = mediator;

    /// <summary>Queues a JSON export to object storage.</summary>
    [HttpPost("data-export")]
    [ProducesResponseType(typeof(ApiResponse<RequestDataExportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RequestDataExportResponse>>> RequestExport(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RequestDataExportCommand(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<RequestDataExportResponse> { Data = result });
    }

    /// <summary>Returns status and pre-signed download URL when <c>ready</c>.</summary>
    [HttpGet("data-export/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DataExportStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DataExportStatusDto>>> GetExport(
        Guid id,
        CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(new GetDataExportByIdQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DataExportStatusDto> { Data = dto });
    }
}
