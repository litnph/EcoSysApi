using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.InstallmentPlans.Common;
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

                var today = FinanceBusinessCalendar.Today;
                var effectiveStatus = InstallmentPaySchedule.ResolveStatus(pay, today);
                if (effectiveStatus == InstallmentPayStatus.Paid)
                    ctx.AddFailure(nameof(cmd.InstallmentNumber), "The installment has already been paid.");
            });

        RuleFor(x => x.PaymentSourceId).MustAsync(
                async (sourceId, ct) =>
                {
                    var source = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sourceId, ct).ConfigureAwait(false);
                    return source is not null && !source.IsDeleted && !source.IsArchived && source.Type != SourceType.CreditCard;
                })
            .WithMessage("Payment source must be an active non-credit-card source.");

        RuleFor(x => x).MustAsync(
                async (cmd, ct) =>
                {
                    var plan = await db.FinInstallmentPlans.AsNoTracking()
                        .Include(p => p.Source)
                        .FirstOrDefaultAsync(p => p.Id == cmd.PlanId, ct)
                        .ConfigureAwait(false);
                    var paymentSource = await db.FinSources.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == cmd.PaymentSourceId, ct)
                        .ConfigureAwait(false);
                    return plan is null || paymentSource is null
                        || string.Equals(plan.Source.Currency, paymentSource.Currency, StringComparison.Ordinal);
                })
            .WithMessage("Payment source currency must match the installment plan currency.");
    }
}
