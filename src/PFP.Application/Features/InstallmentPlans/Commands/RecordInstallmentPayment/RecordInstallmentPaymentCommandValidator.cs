using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Commands.RecordInstallmentPayment;

/// <summary>FluentValidation rules for <see cref="RecordInstallmentPaymentCommand"/>.</summary>
public sealed class RecordInstallmentPaymentCommandValidator : AbstractValidator<RecordInstallmentPaymentCommand>
{
    /// <summary>Creates the validator.</summary>
    public RecordInstallmentPaymentCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.InstallmentNumber).GreaterThan(0);

        RuleFor(x => x.PlanId).MustAsync(
                async (planId, ct) =>
                {
                    var plan = await db.FinInstallmentPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == planId, ct).ConfigureAwait(false);
                    return plan is not null && plan.Status == InstallmentStatus.Active;
                })
            .WithMessage("The installment plan must exist and be active.");

        RuleFor(x => x).CustomAsync(
            async (cmd, ctx, ct) =>
            {
                var pay = await db.FinInstallmentPays
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        p => p.PlanId == cmd.PlanId && p.InstallmentNumber == cmd.InstallmentNumber,
                        ct)
                    .ConfigureAwait(false);

                if (pay is null)
                {
                    ctx.AddFailure("InstallmentNumber", "Installment pay row was not found.");
                    return;
                }

                if (pay.Status is not (InstallmentPayStatus.Due or InstallmentPayStatus.Overdue))
                    ctx.AddFailure(nameof(cmd.InstallmentNumber), "The installment must be due or overdue to accept a payment.");
            });

        RuleFor(x => x.PaymentSourceId).MustAsync(
                async (sourceId, ct) =>
                {
                    var exists = await db.FinSources.AsNoTracking().AnyAsync(s => s.Id == sourceId, ct).ConfigureAwait(false);
                    return exists;
                })
            .WithMessage("Payment source was not found.");
    }
}
