using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Commands.CancelInstallmentPlan;

/// <summary>FluentValidation rules for <see cref="CancelInstallmentPlanCommand"/>.</summary>
public sealed class CancelInstallmentPlanCommandValidator : AbstractValidator<CancelInstallmentPlanCommand>
{
    /// <summary>Creates the validator.</summary>
    public CancelInstallmentPlanCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.PlanId).MustAsync(
                async (planId, ct) =>
                {
                    var plan = await db.FinInstallmentPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == planId, ct).ConfigureAwait(false);
                    return plan is not null && plan.Status == InstallmentStatus.Active;
                })
            .WithMessage("The installment plan must exist and be active.");
    }
}
