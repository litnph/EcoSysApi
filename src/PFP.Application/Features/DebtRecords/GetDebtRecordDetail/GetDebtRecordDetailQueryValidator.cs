using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.DebtRecords.GetDebtRecordDetail;

public sealed class GetDebtRecordDetailQueryValidator : AbstractValidator<GetDebtRecordDetailQuery>
{
    public GetDebtRecordDetailQueryValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.DebtRecordId).NotEmpty();

        RuleFor(x => x)
            .MustAsync(async (q, ct) =>
                await db.FinDebtRecords.AsNoTracking().AnyAsync(r => r.Id == q.DebtRecordId, ct).ConfigureAwait(false))
            .WithMessage("Debt record was not found.");
    }
}
