using FluentValidation;

namespace PFP.Application.Features.MonthlyPeriods.RefreshMonthlyReport;

/// <summary>Validates <see cref="RefreshMonthlyReportCommand"/>.</summary>
public sealed class RefreshMonthlyReportCommandValidator : AbstractValidator<RefreshMonthlyReportCommand>
{
    public RefreshMonthlyReportCommandValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
