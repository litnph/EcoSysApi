using FluentValidation;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReportAddableTransactions;

public sealed class GetMonthlyReportAddableTransactionsQueryValidator
    : AbstractValidator<GetMonthlyReportAddableTransactionsQuery>
{
    public GetMonthlyReportAddableTransactionsQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
