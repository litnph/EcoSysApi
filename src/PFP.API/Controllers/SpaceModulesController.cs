using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.SpaceModules.GetSpaceModules;
using PFP.Application.Features.SpaceModules.ToggleSpaceModule;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Enables / disables and lists modules on a single space.</summary>
[ApiController]
[Authorize]
[Route("api/v1/spaces/{spaceId:guid}/modules")]
public sealed class SpaceModulesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public SpaceModulesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists every module attached to the space.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetSpaceModulesResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSpaceModulesResponse>>> List(
        Guid spaceId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSpaceModulesQuery(spaceId), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetSpaceModulesResponse> { Data = result });
    }

    /// <summary>Enables or disables one module on the space (idempotent).</summary>
    [HttpPut("{moduleCode}")]
    [ProducesResponseType(typeof(ApiResponse<ToggleSpaceModuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ToggleSpaceModuleResponse>>> Toggle(
        Guid spaceId,
        ModuleCode moduleCode,
        [FromBody] ToggleSpaceModuleBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new ToggleSpaceModuleCommand(spaceId, moduleCode, body.Enable), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<ToggleSpaceModuleResponse> { Data = result });
    }
}

/// <summary>JSON body for <see cref="SpaceModulesController.Toggle"/>.</summary>
public sealed class ToggleSpaceModuleBody
{
    /// <summary><c>true</c> to enable the module, <c>false</c> to disable it.</summary>
    public bool Enable { get; init; }
}
