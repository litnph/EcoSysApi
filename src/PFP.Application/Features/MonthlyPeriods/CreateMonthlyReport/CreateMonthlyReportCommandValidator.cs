using FluentValidation;

namespace PFP.Application.Features.MonthlyPeriods.CreateMonthlyReport;

/// <summary>Validates <see cref="CreateMonthlyReportCommand"/>.</summary>
public sealed class CreateMonthlyReportCommandValidator : AbstractValidator<CreateMonthlyReportCommand>
{
    public CreateMonthlyReportCommandValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
