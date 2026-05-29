using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Commands.CreateInstallmentPlan;

/// <summary>FluentValidation rules for <see cref="CreateInstallmentPlanCommand"/>.</summary>
public sealed class CreateInstallmentPlanCommandValidator : AbstractValidator<CreateInstallmentPlanCommand>
{
    /// <summary>Creates the validator.</summary>
    public CreateInstallmentPlanCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.TotalMonths).InclusiveBetween(3, 60);

        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0);

        RuleFor(x => x.ConversionFeeRate)
            .GreaterThanOrEqualTo(0m)
            .When(x => x.ConversionFeeRate is not null);

        RuleFor(x => x.ConversionFeeRate)
            .Must(r => r is null)
            .When(x => x.InterestRate > 0m);

        RuleFor(x => x.OriginalTxnId).MustAsync(
                async (txnId, ct) =>
                {
                    var txn = await db.FinTransactions
                        .AsNoTracking()
                        .Include(t => t.Source)
                        .FirstOrDefaultAsync(t => t.Id == txnId, ct)
                        .ConfigureAwait(false);

                    if (txn is null || txn.IsDeleted || txn.Type != TransactionType.Deferred)
                        return false;

                    if (txn.Source.Type != SourceType.CreditCard)
                        return false;

                    var minAmt = txn.Source.MinInstallmentAmt;
                    if (minAmt is { } m && txn.Amount < m)
                        return false;

                    var hasActive = await db.FinInstallmentPlans
                        .AsNoTracking()
                        .AnyAsync(
                            p => p.OriginalTxnId == txnId && p.Status == InstallmentStatus.Active,
                            ct)
                        .ConfigureAwait(false);

                    return !hasActive;
                })
            .WithMessage(
                "The transaction must exist, be deferred on a credit card, meet minimum installment amount when configured, and must not already have an active installment plan.");
    }
}
