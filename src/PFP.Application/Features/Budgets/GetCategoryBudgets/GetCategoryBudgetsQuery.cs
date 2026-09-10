using MediatR;
using PFP.Application.Features.Budgets.Common;

namespace PFP.Application.Features.Budgets.GetCategoryBudgets;

public sealed record GetCategoryBudgetsQuery(int? Year = null, int? Month = null)
    : IRequest<GetCategoryBudgetsResponse>;

public sealed record GetCategoryBudgetsResponse(CategoryBudgetsDto Budgets);
