using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using ApiResults = PFP.API.Models.ApiResults;
using PFP.Application.Features.Transactions.Common;
using PFP.Application.Features.Transactions.CreateTransaction;
using PFP.Application.Features.Transactions.GetTransactionById;
using PFP.Application.Features.Transactions.GetTransactions;
using PFP.Application.Features.Transactions.DeleteTransaction;
using PFP.Application.Features.Transactions.GetTransactionHistory;
using PFP.Application.Features.Transactions.UpdateTransaction;
using PFP.Application.Features.FileAttachments.Common;
using PFP.Application.Features.FileAttachments.ListTransactionAttachments;
using PFP.Application.Features.Transactions.GetTransferPair;
using PFP.Application.Features.Transactions.Splits.GetSplitsByTransaction;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Finance transactions (MVP: list, create, delete, detail, transfer pair, history).</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public TransactionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists transactions with optional filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetTransactionsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetTransactionsResponse>>> List(
        [FromQuery(Name = "source_id")] Guid? sourceId,
        [FromQuery(Name = "type")] TransactionType? type,
        [FromQuery(Name = "category_id")] Guid? categoryId,
        [FromQuery(Name = "date_from")] DateOnly? dateFrom,
        [FromQuery(Name = "date_to")] DateOnly? dateTo,
        [FromQuery(Name = "amount_min")] long? amountMin,
        [FromQuery(Name = "amount_max")] long? amountMax,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionsQuery(sourceId,
            type,
            categoryId,
            dateFrom,
            dateTo,
            amountMin,
            amountMax,
            page,
            pageSize);

        var result = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResults.Ok(
            result,
            new
            {
                page = result.Page,
                pageSize = result.PageSize,
                totalCount = result.TotalCount,
                totalPages = result.TotalPages,
            }));
    }

    /// <summary>Creates a transaction (direct, income, transfer, or deferred).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateTransactionResponse>>> Create(
        [FromBody] CreateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateTransactionResponse> { Data = result });
    }

    /// <summary>Returns transaction detail including source and category.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetTransactionByIdResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetTransactionByIdResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTransactionByIdQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetTransactionByIdResponse> { Data = result });
    }

    /// <summary>Lists attachments stored for this transaction (metadata only — use files API for download URLs).</summary>
    [HttpGet("{id:guid}/attachments")]
    [ProducesResponseType(typeof(ApiResponse<GetTransactionAttachmentsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetTransactionAttachmentsResponse>>> ListAttachments(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTransactionAttachmentsQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetTransactionAttachmentsResponse> { Data = result });
    }

    /// <summary>Lists split participant rows for a transaction.</summary>
    [HttpGet("{id:guid}/splits")]
    [ProducesResponseType(typeof(ApiResponse<GetSplitsByTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSplitsByTransactionResponse>>> GetSplits(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSplitsByTransactionQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetSplitsByTransactionResponse> { Data = result });
    }

    /// <summary>Returns both FIN_TRANSACTIONS rows for a transfer (outbound then inbound).</summary>
    [HttpGet("{id:guid}/transfer-pair")]
    [ProducesResponseType(typeof(ApiResponse<GetTransferPairResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetTransferPairResponse>>> GetTransferPair(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTransferPairQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetTransferPairResponse> { Data = result });
    }

    /// <summary>Returns version history rows for a transaction (oldest version first).</summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<GetTransactionHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetTransactionHistoryResponse>>> GetHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTransactionHistoryQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetTransactionHistoryResponse> { Data = result });
    }

    /// <summary>Updates transaction metadata (description, note, category, txn date, monthly period).</summary>
    /// <remarks>
    /// Balance-affecting fields (amount, type, source) are intentionally not exposed — to change
    /// those, delete the transaction (which posts a reversal) and re-create it.
    /// </remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateTransactionResponse>>> Update(
        Guid id,
        [FromBody] UpdateTransactionBody body,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTransactionCommand(
            id,
            body.CategoryId,
            body.TxnDate,
            body.Description,
            body.Note,
            body.MonthlyPeriodId);

        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateTransactionResponse> { Data = result });
    }

    /// <summary>Soft-deletes a transaction and posts reversal row(s).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteTransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteTransactionResponse>>> Delete(
        Guid id,
        [FromBody] DeleteTransactionRequest? body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteTransactionCommand(id, body?.Reason), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DeleteTransactionResponse> { Data = result });
    }
}

/// <summary>JSON body for <see cref="TransactionsController.Update"/>.</summary>
public sealed class UpdateTransactionBody
{
    /// <summary>Optional new category id (must belong to the same finance module).</summary>
    public Guid? CategoryId { get; init; }

    /// <summary>New business date of the transaction.</summary>
    public DateOnly TxnDate { get; init; }

    /// <summary>Human-readable description (max 512 chars).</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Optional note (max 500 chars).</summary>
    public string? Note { get; init; }

    /// <summary>Optional monthly-period reassignment.</summary>
    public Guid? MonthlyPeriodId { get; init; }
}
