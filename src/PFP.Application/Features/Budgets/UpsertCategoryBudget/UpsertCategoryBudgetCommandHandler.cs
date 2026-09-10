using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Budgets.Common;
using PFP.Application.Features.Notifications.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Budgets.UpsertCategoryBudget;

public sealed class UpsertCategoryBudgetCommandHandler
    : IRequestHandler<UpsertCategoryBudgetCommand, UpsertCategoryBudgetResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IBudgetAlertEvaluator _alerts;

    public UpsertCategoryBudgetCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IBudgetAlertEvaluator alerts)
    {
        _db = db;
        _currentUser = currentUser;
        _alerts = alerts;
    }

    public async Task<UpsertCategoryBudgetResponse> Handle(
        UpsertCategoryBudgetCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;
        var normalizedThresholds = BudgetThresholds.Normalize(request.WarningThresholds);
        FinCategoryBudget? budget = null;

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            // The category row is the stable serialization point even before a
            // user/category budget row exists. This prevents concurrent first
            // writes from racing into the unique index and prevents existing
            // configurations from losing a version increment.
            var category = await _db.FinCategories
                .FromSqlInterpolated(
                    $"SELECT * FROM fin_categories WITH (UPDLOCK, ROWLOCK) WHERE id = {request.CategoryId}")
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
            if (category is null)
                throw new NotFoundException("Category was not found.");
            if (category.Kind != CategoryKind.Expense)
                throw new BusinessRuleException("Budgets can only be configured for expense categories.");

            budget = await _db.FinCategoryBudgets
                .FirstOrDefaultAsync(
                    row => row.UserId == userId && row.CategoryId == request.CategoryId,
                    ct)
                .ConfigureAwait(false);

            if (budget is null)
            {
                budget = new FinCategoryBudget
                {
                    UserId = userId,
                    CategoryId = request.CategoryId,
                };
                _db.FinCategoryBudgets.Add(budget);
            }
            else
            {
                budget.ConfigurationVersion += 1;
            }

            budget.IsEnabled = request.IsEnabled;
            budget.Amount = request.IsEnabled
                ? CurrencyUnits.FromWhole(request.BudgetAmount)
                : Math.Max(0m, CurrencyUnits.FromWhole(request.BudgetAmount));
            budget.Currency = request.Currency.Trim().ToUpperInvariant();
            budget.TargetMode = request.TargetMode;
            budget.WarningThresholdsJson = BudgetThresholdJson.Serialize(normalizedThresholds);

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            if (budget.IsEnabled)
                await _alerts.EvaluateCurrentCycleAsync(userId, ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new UpsertCategoryBudgetResponse(budget!.Id, budget.ConfigurationVersion);
    }
}
