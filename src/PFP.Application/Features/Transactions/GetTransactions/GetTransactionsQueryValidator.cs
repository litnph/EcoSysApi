using FluentValidation;

namespace PFP.Application.Features.Transactions.GetTransactions;

/// <summary>FluentValidation rules for <see cref="GetTransactionsQuery"/>.</summary>
public sealed class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetTransactionsQueryValidator()
    {
        RuleFor(x => x.SmoduleId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
