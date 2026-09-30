using MediatR;
using PFP.Application.Features.Transactions.Common;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReportAddableTransactions;

public sealed record GetMonthlyReportAddableTransactionsQuery(int Year, int Month)
    : IRequest<GetMonthlyReportAddableTransactionsResponse>;

public sealed record GetMonthlyReportAddableTransactionsResponse(
    IReadOnlyList<TransactionListItemDto> Items);
