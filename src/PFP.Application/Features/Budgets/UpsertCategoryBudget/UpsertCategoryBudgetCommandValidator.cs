using FluentValidation;
using PFP.Application.Features.Budgets.Common;

namespace PFP.Application.Features.Budgets.UpsertCategoryBudget;

public sealed class UpsertCategoryBudgetCommandValidator : AbstractValidator<UpsertCategoryBudgetCommand>
{
    public UpsertCategoryBudgetCommandValidator()
    {
        RuleFor(command => command.CategoryId).NotEmpty();
        RuleFor(command => command.Currency)
            .NotEmpty()
            .Matches("^[A-Za-z]{3,8}$");
        RuleFor(command => command.BudgetAmount)
            .GreaterThan(0)
            .When(command => command.IsEnabled);
        RuleFor(command => command.TargetMode).IsInEnum();
        RuleFor(command => command.WarningCount)
            .InclusiveBetween(0, BudgetThresholds.MaximumWarningCount);
        RuleFor(command => command.WarningThresholds)
            .NotNull()
            .Must((command, thresholds) => thresholds is not null
                && BudgetThresholds.IsValid(thresholds, command.WarningCount))
            .WithMessage("Warning thresholds must match the count, be strictly increasing, unique, and between 0 and 100.");
    }
}
