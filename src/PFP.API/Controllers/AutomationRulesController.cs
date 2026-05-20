using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Automation.CreateAutomationRule;
using PFP.Application.Features.Automation.DeleteAutomationRule;
using PFP.Application.Features.Automation.GetAutomationRuleDetail;
using PFP.Application.Features.Automation.GetAutomationRuleLogs;
using PFP.Application.Features.Automation.GetAutomationRules;
using PFP.Application.Features.Automation.ToggleAutomationRule;
using PFP.Application.Features.Automation.UpdateAutomationRule;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Finance automation rules.</summary>
[ApiController]
[Authorize]
[Route("api/v1/automation/rules")]
public sealed class AutomationRulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AutomationRulesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetAutomationRulesResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetAutomationRulesResponse>>> List(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAutomationRulesQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetAutomationRulesResponse> { Data = result });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetAutomationRuleDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetAutomationRuleDetailResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAutomationRuleDetailQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetAutomationRuleDetailResponse> { Data = result });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateAutomationRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateAutomationRuleResponse>>> Create(
        [FromBody] CreateAutomationRuleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateAutomationRuleResponse> { Data = result });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateAutomationRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateAutomationRuleResponse>>> Update(
        Guid id,
        [FromBody] UpdateAutomationRuleBody body,
        CancellationToken cancellationToken)
    {
        var cmd = new UpdateAutomationRuleCommand(
            id,
            body.Name,
            body.TriggerType,
            body.TriggerValue,
            body.Conditions,
            body.Actions,
            body.IsActive);
        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateAutomationRuleResponse> { Data = result });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteAutomationRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteAutomationRuleResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteAutomationRuleCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DeleteAutomationRuleResponse> { Data = result });
    }

    [HttpPost("{id:guid}/toggle")]
    [ProducesResponseType(typeof(ApiResponse<ToggleAutomationRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ToggleAutomationRuleResponse>>> Toggle(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleAutomationRuleCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<ToggleAutomationRuleResponse> { Data = result });
    }

    [HttpGet("{id:guid}/logs")]
    [ProducesResponseType(typeof(ApiResponse<GetAutomationRuleLogsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetAutomationRuleLogsResponse>>> Logs(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator
            .Send(new GetAutomationRuleLogsQuery(id, page, pageSize), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<GetAutomationRuleLogsResponse> { Data = result });
    }
}

/// <summary>JSON body for PUT <c>/api/v1/automation/rules/{id}</c>.</summary>
public sealed record UpdateAutomationRuleBody(
    string Name,
    TriggerType TriggerType,
    string TriggerValue,
    string Conditions,
    string Actions,
    bool IsActive);
