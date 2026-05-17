using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Common.Models;
using PFP.Application.Features.FeatureFlags.CreateFeatureFlag;
using PFP.Application.Features.FeatureFlags.CreateFeatureFlagOverride;
using PFP.Application.Features.FeatureFlags.DeleteFeatureFlagOverride;
using PFP.Application.Features.FeatureFlags.GetFeatureFlagsForCurrentUser;
using PFP.Application.Features.FeatureFlags.UpdateFeatureFlag;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Feature flags for front-end bootstrap and platform admin CRUD.</summary>
[ApiController]
[Route("api/v1/feature-flags")]
public sealed class FeatureFlagsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public FeatureFlagsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns every persisted flag with its resolved enabled state for the JWT principal.</summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FeatureFlagForPrincipalDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FeatureFlagForPrincipalDto>>>> ListForCurrentUser(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetFeatureFlagsForCurrentUserQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<IReadOnlyList<FeatureFlagForPrincipalDto>> { Data = rows });
    }

    /// <summary>Registers a new feature flag (<c>key</c> must be globally unique).</summary>
    [HttpPost]
    [Authorize(Policy = "PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CreateFeatureFlagResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateFeatureFlagResponse>>> Create(
        [FromBody] CreateFeatureFlagCommand command,
        CancellationToken cancellationToken)
    {
        var created = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateFeatureFlagResponse> { Data = created });
    }

    /// <summary>Updates flag metadata / defaults (does not mutate <see cref="PFP.Domain.Entities.FeatureFlag.Key"/>).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<UpdateFeatureFlagResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateFeatureFlagResponse>>> Update(
        Guid id,
        [FromBody] UpdateFeatureFlagBody body,
        CancellationToken cancellationToken)
    {
        if (body is null)
            return BadRequest(new ApiResponse<UpdateFeatureFlagResponse> { Success = false, Error = new { message = "Body is required." } });

        var command = new UpdateFeatureFlagCommand(
            id,
            body.Name,
            body.Description,
            body.IsEnabledGlobal,
            body.RolloutPercentage,
            body.IsArchived);

        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateFeatureFlagResponse> { Data = result });
    }

    /// <summary>Adds or replaces behaviour for one user/org via overrides (evaluated ahead of rollout / global).</summary>
    [HttpPost("{id:guid}/overrides")]
    [Authorize(Policy = "PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CreateFeatureFlagOverrideResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateFeatureFlagOverrideResponse>>> CreateOverride(
        Guid id,
        [FromBody] CreateFeatureFlagOverrideRequest body,
        CancellationToken cancellationToken)
    {
        if (body is null)
            return BadRequest(new ApiResponse<CreateFeatureFlagOverrideResponse> { Success = false, Error = new { message = "Body is required." } });

        var command = new CreateFeatureFlagOverrideCommand(
            id,
            body.TargetType,
            body.TargetId,
            body.IsEnabled,
            body.ExpiresAt);

        var created = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateFeatureFlagOverrideResponse> { Data = created });
    }

    /// <summary>Removes a single override row.</summary>
    [HttpDelete("{id:guid}/overrides/{overrideId:guid}")]
    [Authorize(Policy = "PlatformAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteOverride(
        Guid id,
        Guid overrideId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteFeatureFlagOverrideCommand(id, overrideId), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { flagId = id, overrideId } });
    }
}

/// <summary>PUT <c>/api/v1/feature-flags/{id}</c> payload.</summary>
public sealed class UpdateFeatureFlagBody
{
    /// <summary>Human-readable title (max 200).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional description (max 500).</summary>
    public string? Description { get; init; }

    /// <summary>Fallback when rollout / overrides do not enable the flag.</summary>
    public bool IsEnabledGlobal { get; init; }

    /// <summary>Deterministic rollout 0–100.</summary>
    public int RolloutPercentage { get; init; }

    /// <summary>Archived flags evaluate to disabled for end users.</summary>
    public bool IsArchived { get; init; }
}

/// <summary>POST override payload (<c>/api/v1/feature-flags/{id}/overrides</c>).</summary>
public sealed record CreateFeatureFlagOverrideRequest(
    OverrideTargetType TargetType,
    Guid TargetId,
    bool IsEnabled,
    DateTime? ExpiresAt);
