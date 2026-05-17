using FluentValidation;

namespace PFP.Application.Features.Transactions.GetTransactionById;

/// <summary>FluentValidation rules for <see cref="GetTransactionByIdQuery"/>.</summary>
public sealed class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetTransactionByIdQueryValidator() =>
        RuleFor(x => x.Id).NotEmpty();
}
