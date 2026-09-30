using FluentValidation;

namespace PFP.Application.Features.MonthlyPeriods.AddMonthlyReportTransaction;

public sealed class AddMonthlyReportTransactionCommandValidator
    : AbstractValidator<AddMonthlyReportTransactionCommand>
{
    public AddMonthlyReportTransactionCommandValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.TransactionId).NotEmpty();
    }
}
