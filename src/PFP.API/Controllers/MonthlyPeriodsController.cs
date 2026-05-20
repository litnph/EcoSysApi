using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.MonthlyPeriods.CloseMonth;
using PFP.Application.Features.MonthlyPeriods.GetCurrentMonthSummary;
using PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriod;
using PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriodsList;
using PFP.Application.Features.MonthlyPeriods.GetMonthlyReport;

namespace PFP.API.Controllers;

/// <summary>Finance monthly periods (summary, close).</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/monthly-periods")]
public sealed class MonthlyPeriodsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public MonthlyPeriodsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Last 12 calendar months for the module (newest first).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetMonthlyPeriodsListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetMonthlyPeriodsListResponse>>> List(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMonthlyPeriodsListQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetMonthlyPeriodsListResponse> { Data = result });
    }

    /// <summary>UTC current calendar month summary for a finance module.</summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<GetCurrentMonthSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetCurrentMonthSummaryResponse>>> GetCurrent(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentMonthSummaryQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetCurrentMonthSummaryResponse> { Data = result });
    }

    /// <summary>Full report for a calendar month (cashflow, breakdowns, comparisons).</summary>
    [HttpGet("{year:int}/{month:int}/report")]
    [ProducesResponseType(typeof(ApiResponse<GetMonthlyReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetMonthlyReportResponse>>> GetReport(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMonthlyReportQuery(year, month), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetMonthlyReportResponse> { Data = result });
    }

    /// <summary>Summary for a specific calendar month.</summary>
    [HttpGet("{year:int}/{month:int}")]
    [ProducesResponseType(typeof(ApiResponse<GetMonthlyPeriodResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetMonthlyPeriodResponse>>> GetByYearMonth(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMonthlyPeriodQuery(year, month), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetMonthlyPeriodResponse> { Data = result });
    }

    /// <summary>Closes a month after billing-cycle validation.</summary>
    [HttpPost("close")]
    [ProducesResponseType(typeof(ApiResponse<CloseMonthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CloseMonthResponse>>> Close(
        [FromBody] CloseMonthRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CloseMonthCommand( body.Year, body.Month), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CloseMonthResponse> { Data = result });
    }
}
