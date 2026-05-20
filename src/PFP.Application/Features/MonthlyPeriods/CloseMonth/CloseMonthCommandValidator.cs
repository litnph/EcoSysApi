using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.CloseMonth;

/// <summary>FluentValidation rules for <see cref="CloseMonthCommand"/>.</summary>
public sealed class CloseMonthCommandValidator : AbstractValidator<CloseMonthCommand>
{
    /// <summary>Registers validation rules.</summary>
    public CloseMonthCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);

        RuleFor(x => x)
            .Must(cmd =>
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var target = PeriodFirstDay(cmd.Year, cmd.Month);
                return target <= today;
            })
            .WithMessage("Cannot close a future month.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var p = await db.FinMonthlyPeriods
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Year == cmd.Year && m.Month == cmd.Month, ct)
                    .ConfigureAwait(false);
                if (p is null)
                    return true;
                return p.Status == PeriodStatus.Open;
            })
            .WithMessage("The monthly period must be open to close it.");
    }

    private static DateOnly PeriodFirstDay(int year, int month) => new(year, month, 1);
}
