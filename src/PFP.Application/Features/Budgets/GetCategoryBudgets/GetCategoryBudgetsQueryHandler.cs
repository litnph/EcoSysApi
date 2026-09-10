using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Budgets.Common;
using PFP.Application.Features.MonthlyPeriods.Common;

namespace PFP.Application.Features.Budgets.GetCategoryBudgets;

public sealed class GetCategoryBudgetsQueryHandler
    : IRequestHandler<GetCategoryBudgetsQuery, GetCategoryBudgetsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCategoryBudgetsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetCategoryBudgetsResponse> Handle(
        GetCategoryBudgetsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;
        var reportDay = await MonthlyReportUserPreferences
            .GetReportDayAsync(_db, userId, cancellationToken)
            .ConfigureAwait(false);
        var target = request.Year.HasValue
            ? (request.Year.Value, request.Month!.Value)
            : ReportingPeriodCalculator.TargetMonthContaining(FinanceBusinessCalendar.Today, reportDay);
        var period = ReportingPeriodCalculator.ForTargetMonth(target.Item1, target.Item2, reportDay);

        var report = await MonthlyPeriodSummaryCalculator
            .BuildReportAsync(_db, target.Item1, target.Item2, userId, reportDay, cancellationToken)
            .ConfigureAwait(false);
        report = await MonthlyReportBudgetProjector
            .ApplyAsync(_db, report, userId, cancellationToken)
            .ConfigureAwait(false);

        var utilization = (report.CurrencyGroups ?? Array.Empty<MonthlyReportCurrencyGroupDto>())
            .SelectMany(group => group.BudgetUtilizations ?? Array.Empty<CategoryBudgetUtilizationDto>())
            .ToDictionary(row => row.CategoryId);

        var budgets = await _db.FinCategoryBudgets
            .AsNoTracking()
            .Where(budget => budget.UserId == userId)
            .OrderBy(budget => budget.Category.Name)
            .Select(budget => new
            {
                budget.CategoryId,
                CategoryName = budget.Category.Name,
                budget.Currency,
                budget.Amount,
                budget.TargetMode,
                budget.IsEnabled,
                budget.WarningThresholdsJson,
                budget.ConfigurationVersion,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = budgets.Select(budget =>
        {
            utilization.TryGetValue(budget.CategoryId, out var current);
            var thresholds = BudgetThresholdJson.Deserialize(budget.WarningThresholdsJson);
            var initial = budget.Amount > 0m
                ? BudgetTargetRules.Evaluate(0m, budget.Amount, budget.TargetMode, thresholds)
                : null;
            return new CategoryBudgetDto(
                budget.CategoryId,
                budget.CategoryName,
                budget.Currency,
                CurrencyUnits.ToWhole(budget.Amount),
                budget.IsEnabled,
                thresholds.Count,
                thresholds,
                current?.SpentAmount ?? 0,
                current?.RemainingAmount
                    ?? CurrencyUnits.ToWhole(initial?.RemainingAmount ?? budget.Amount),
                current?.UtilizationPercent ?? initial?.ProgressPercent ?? 0m,
                current?.Status
                    ?? initial?.Status
                    ?? (budget.TargetMode == PFP.Domain.Enums.BudgetTargetMode.Minimum
                        ? BudgetTargetRules.BelowTargetStatus
                        : BudgetTargetRules.WithinBudgetStatus),
                budget.ConfigurationVersion,
                budget.TargetMode);
        }).ToList();

        return new GetCategoryBudgetsResponse(new CategoryBudgetsDto(
            target.Item1,
            target.Item2,
            reportDay,
            period.StartInclusive,
            period.EndExclusive,
            items));
    }
}
