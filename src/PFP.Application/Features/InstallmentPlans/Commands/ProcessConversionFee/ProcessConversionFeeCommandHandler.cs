using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;
using PFP.Application.Common.Utils;
using PFP.Domain.Entities;

namespace PFP.Application.Features.InstallmentPlans.Commands.ProcessConversionFee;

/// <summary>Emits deferred fee transactions for installment plans tied to the billing cycle's card.</summary>
public sealed class ProcessConversionFeeCommandHandler : IRequestHandler<ProcessConversionFeeCommand, ProcessConversionFeeResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ProcessConversionFeeCommandHandler> _logger;

    /// <summary>Creates the handler.</summary>
    public ProcessConversionFeeCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<ProcessConversionFeeCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProcessConversionFeeResponse> Handle(
        ProcessConversionFeeCommand request,
        CancellationToken cancellationToken)
    {
        var cycle = await _db.FinBillingCycles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.BillingCycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
            return new ProcessConversionFeeResponse(0, 0);

        var plans = await _db.FinInstallmentPlans
            .Where(p =>
                p.SourceId == cycle.SourceId
                && p.ConversionFeeStatus == ConversionFeeStatus.Pending
                && p.ConversionFeeAmount != null
                && p.ConversionFeeAmount > 0)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var processed = 0;
        var errors = 0;

        foreach (var plan in plans)
        {
            try
            {
                await DbTransactionRunner.ExecuteAsync(_db, async ct =>
                {
                    var trackedPlan = await _db.FinInstallmentPlans
                        .FirstAsync(p => p.Id == plan.Id, ct)
                        .ConfigureAwait(false);

                    if (trackedPlan.ConversionFeeStatus != ConversionFeeStatus.Pending
                        || trackedPlan.ConversionFeeAmount is not { } feeAmt)
                        return;

                    var trackedCycle = await _db.FinBillingCycles
                        .FirstAsync(c => c.Id == request.BillingCycleId, ct)
                        .ConfigureAwait(false);

                    var source = await _db.FinSources
                        .FirstAsync(s => s.Id == trackedPlan.SourceId, ct)
                        .ConfigureAwait(false);

                    var category = await _db.FinCategories
                        .OrderByDescending(c => c.IsDefault)
                        .ThenBy(c => c.SortOrder)
                        .FirstOrDefaultAsync(ct)
                        .ConfigureAwait(false);

                    if (category is null)
                        throw new InvalidOperationException("No expense category exists for this finance module.");

                    var note = "Phí chuyển đổi trả góp";
                    var description = $"Expense: {category.Name}";
                    if (description.Length > 512)
                        description = description[..512];

                    var feeTxn = new FinTransaction
                    {
                        Type = TransactionType.Deferred,
                        Status = TxnStatus.New,
                        Amount = feeAmt,
                        Currency = source.Currency,
                        TxnDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        SourceId = trackedPlan.SourceId,
                        CategoryId = category.Id,
                        Description = description,
                        Note = note.Length <= 500 ? note : note[..500],
                        InstallmentPlanId = trackedPlan.Id,
                    };

                    _db.FinTransactions.Add(feeTxn);

                    source.Balance += feeAmt;

                    if (trackedCycle.Status == BillingCycleStatus.Open)
                    {
                        _db.FinBillingCycleItems.Add(new FinBillingCycleItem
                        {
                            BillingCycleId = trackedCycle.Id,
                            TransactionId = feeTxn.Id,
                            InclusionSource = BillingCycleItemInclusionSource.Refresh,
                        });
                        await BillingCycleTotals.RecalculateAsync(trackedCycle, _db, ct)
                            .ConfigureAwait(false);
                    }

                    trackedPlan.ConversionFeeStatus = ConversionFeeStatus.Billed;
                    trackedPlan.ConversionFeeTxnId = feeTxn.Id;

                    FinTransactionHistoryHelper.AddCreated(_db, _currentUser, feeTxn);

                    await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                }, cancellationToken).ConfigureAwait(false);

                processed++;
            }
            catch (Exception ex)
            {
                errors++;
                _logger.LogError(ex, "ProcessConversionFee failed for plan {PlanId}", plan.Id);
            }
        }

        return new ProcessConversionFeeResponse(processed, errors);
    }
}
