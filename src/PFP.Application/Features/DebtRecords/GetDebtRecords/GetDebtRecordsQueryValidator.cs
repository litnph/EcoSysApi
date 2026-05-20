using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.DebtRecords.GetDebtRecords;

public sealed class GetDebtRecordsQueryValidator : AbstractValidator<GetDebtRecordsQuery>
{
    public GetDebtRecordsQueryValidator(IApplicationDbContext db)
    {
}
}
