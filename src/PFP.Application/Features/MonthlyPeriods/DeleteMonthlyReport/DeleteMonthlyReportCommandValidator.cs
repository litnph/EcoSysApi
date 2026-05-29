using FluentValidation;

namespace PFP.Application.Features.MonthlyPeriods.DeleteMonthlyReport;

/// <summary>Validates <see cref="DeleteMonthlyReportCommand"/>.</summary>
public sealed class DeleteMonthlyReportCommandValidator : AbstractValidator<DeleteMonthlyReportCommand>
{
    public DeleteMonthlyReportCommandValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
