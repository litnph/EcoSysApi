using MediatR;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.GetTransactions;

/// <summary>Lists transactions with optional filters and pagination.</summary>
public sealed record GetTransactionsQuery(
    Guid? SourceId,
    TransactionType? Type,
    Guid? CategoryId,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    long? AmountMin,
    long? AmountMax,
    int Page = 1,
    int PageSize = 20) : IRequest<GetTransactionsResponse>;
