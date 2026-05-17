using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.DeleteDebtRecord;

public sealed class DeleteDebtRecordCommandValidator : AbstractValidator<DeleteDebtRecordCommand>
{
    public DeleteDebtRecordCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.DebtRecordId).NotEmpty();

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var r = await db.FinDebtRecords
                    .Include(x => x.FinDebtTransactions)
                    .FirstOrDefaultAsync(x => x.Id == cmd.DebtRecordId, ct)
                    .ConfigureAwait(false);
                if (r is null || r.IsDeleted)
                    return false;
                if (r.Status != DebtStatus.Active)
                    return false;
                return r.FinDebtTransactions.Count == 0;
            })
            .WithMessage("Only an active debt record with no ledger movements can be removed as a mistaken entry.");
    }
}
