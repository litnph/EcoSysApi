using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.DebtRecords.GetDebtRecords;

public sealed class GetDebtRecordsQueryValidator : AbstractValidator<GetDebtRecordsQuery>
{
    public GetDebtRecordsQueryValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SmoduleId).NotEmpty();

        RuleFor(x => x)
            .MustAsync(async (q, ct) =>
                await db.SpaceModules.AnyAsync(m => m.Id == q.SmoduleId, ct).ConfigureAwait(false))
            .WithMessage("Space module was not found.");
    }
}
