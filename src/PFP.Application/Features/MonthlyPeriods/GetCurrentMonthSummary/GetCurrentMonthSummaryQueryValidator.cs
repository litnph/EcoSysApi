using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetCurrentMonthSummary;

/// <summary>FluentValidation rules for <see cref="GetCurrentMonthSummaryQuery"/>.</summary>
public sealed class GetCurrentMonthSummaryQueryValidator : AbstractValidator<GetCurrentMonthSummaryQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetCurrentMonthSummaryQueryValidator(IApplicationDbContext db)
    {
}
}
