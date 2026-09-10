using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Budgets.UpsertCategoryBudget;

public sealed record UpsertCategoryBudgetCommand(
    Guid CategoryId,
    bool IsEnabled,
    long BudgetAmount,
    string Currency,
    int WarningCount,
    IReadOnlyList<decimal> WarningThresholds,
    BudgetTargetMode TargetMode) : IRequest<UpsertCategoryBudgetResponse>;

public sealed record UpsertCategoryBudgetResponse(Guid BudgetId, int ConfigurationVersion);
