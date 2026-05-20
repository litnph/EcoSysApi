using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.DebtRecords.GetDebtSummary;

public sealed class GetDebtSummaryQueryValidator : AbstractValidator<GetDebtSummaryQuery>
{
    public GetDebtSummaryQueryValidator(IApplicationDbContext db)
    {
}
}
