using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Organizations.CreateOrganization;
using PFP.Application.Features.Organizations.DeleteOrganization;
using PFP.Application.Features.Organizations.GetMyOrganizations;
using PFP.Application.Features.Organizations.GetOrganizationDetail;
using PFP.Application.Features.Organizations.LeaveOrganization;
using PFP.Application.Features.Organizations.UpdateOrganization;

namespace PFP.API.Controllers;

/// <summary>Organisation lifecycle endpoints (create / list / detail / update / delete / leave).</summary>
[ApiController]
[Authorize]
[Route("api/v1/organizations")]
public sealed class OrganizationsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public OrganizationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists every active organisation the caller belongs to (including the personal one).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetMyOrganizationsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetMyOrganizationsResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyOrganizationsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetMyOrganizationsResponse> { Data = result });
    }

    /// <summary>Returns the detail of one organisation (caller must be a member).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetOrganizationDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetOrganizationDetailResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrganizationDetailQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetOrganizationDetailResponse> { Data = result });
    }

    /// <summary>Creates a non-personal organisation owned by the caller.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateOrganizationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateOrganizationResponse>>> Create(
        [FromBody] CreateOrganizationBody body,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrganizationCommand(body.Name, body.Slug, body.DefaultCurrency, body.Description);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateOrganizationResponse> { Data = result });
    }

    /// <summary>Updates organisation metadata (admins / owner only).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateOrganizationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateOrganizationResponse>>> Update(
        Guid id,
        [FromBody] UpdateOrganizationBody body,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrganizationCommand(id, body.Name, body.DefaultCurrency, body.Description);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateOrganizationResponse> { Data = result });
    }

    /// <summary>Deletes a non-personal organisation (owner only).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteOrganizationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteOrganizationResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteOrganizationCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DeleteOrganizationResponse> { Data = result });
    }

    /// <summary>Lets the caller leave a non-personal organisation.</summary>
    [HttpPost("{id:guid}/leave")]
    [ProducesResponseType(typeof(ApiResponse<LeaveOrganizationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LeaveOrganizationResponse>>> Leave(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LeaveOrganizationCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<LeaveOrganizationResponse> { Data = result });
    }
}

/// <summary>JSON body for <see cref="OrganizationsController.Create"/>.</summary>
public sealed class CreateOrganizationBody
{
    /// <summary>Display name (max 120 chars).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>URL-safe slug (lowercase, alphanumeric, dashes; max 64 chars).</summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>Optional ISO-4217 currency code (defaults to <c>VND</c>).</summary>
    public string? DefaultCurrency { get; init; }

    /// <summary>Optional description shown on the profile page.</summary>
    public string? Description { get; init; }
}

/// <summary>JSON body for <see cref="OrganizationsController.Update"/>.</summary>
public sealed class UpdateOrganizationBody
{
    /// <summary>New display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional new default currency.</summary>
    public string? DefaultCurrency { get; init; }

    /// <summary>Optional new description.</summary>
    public string? Description { get; init; }
}
