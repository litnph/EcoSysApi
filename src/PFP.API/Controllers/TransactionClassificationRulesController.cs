using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.TransactionClassificationRules;

namespace PFP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/finance/transaction-classification-rules")]
public sealed class TransactionClassificationRulesController : ControllerBase
{
    private readonly IMediator _mediator;
    public TransactionClassificationRulesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClassificationRuleDto>>>> List(CancellationToken ct) =>
        Ok(new ApiResponse<IReadOnlyList<ClassificationRuleDto>> { Data = await _mediator.Send(new ListClassificationRulesQuery(), ct) });

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClassificationRuleDto>>> Create(
        [FromBody] CreateClassificationRuleCommand command, CancellationToken ct) =>
        Ok(new ApiResponse<ClassificationRuleDto> { Data = await _mediator.Send(command, ct) });

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ClassificationRuleDto>>> Update(
        Guid id, [FromBody] ClassificationRuleBody body, CancellationToken ct) =>
        Ok(new ApiResponse<ClassificationRuleDto>
        {
            Data = await _mediator.Send(new UpdateClassificationRuleCommand(
                id, body.Keyword, body.CategoryId, body.TagId, body.IsActive), ct)
        });

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteClassificationRuleCommand(id), ct);
        return Ok(new ApiResponse<object> { Data = new { id } });
    }

    [HttpPost("match")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClassificationResult>>>> Match(
        [FromBody] MatchTransactionContentsCommand command, CancellationToken ct) =>
        Ok(new ApiResponse<IReadOnlyList<ClassificationResult>> { Data = await _mediator.Send(command, ct) });
}

public sealed record ClassificationRuleBody(string Keyword, Guid CategoryId, Guid? TagId, bool IsActive);
