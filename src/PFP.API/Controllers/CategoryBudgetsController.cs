using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Budgets.GetCategoryBudgets;
using PFP.Application.Features.Budgets.UpsertCategoryBudget;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/finance/category-budgets")]
public sealed class CategoryBudgetsController : ControllerBase
{
    private readonly IMediator _mediator;
    public CategoryBudgetsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetCategoryBudgetsResponse>>> List(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCategoryBudgetsQuery(year, month), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetCategoryBudgetsResponse> { Data = result });
    }

    [HttpPut("{categoryId:guid}")]
    public async Task<ActionResult<ApiResponse<UpsertCategoryBudgetResponse>>> Upsert(
        Guid categoryId,
        [FromBody] UpsertCategoryBudgetBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpsertCategoryBudgetCommand(
            categoryId,
            body.IsEnabled,
            body.BudgetAmount,
            body.Currency,
            body.WarningCount,
            body.WarningThresholds ?? Array.Empty<decimal>(),
            body.TargetMode), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpsertCategoryBudgetResponse> { Data = result });
    }
}

public sealed class UpsertCategoryBudgetBody
{
    public bool IsEnabled { get; init; }
    public long BudgetAmount { get; init; }
    public string Currency { get; init; } = "VND";
    public int WarningCount { get; init; }
    public IReadOnlyList<decimal>? WarningThresholds { get; init; }
    public BudgetTargetMode TargetMode { get; init; } = BudgetTargetMode.Maximum;
}
