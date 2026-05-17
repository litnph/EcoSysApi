using FluentValidation;

namespace PFP.Application.Features.Investments.DeleteInvestment;

public sealed class DeleteInvestmentCommandValidator : AbstractValidator<DeleteInvestmentCommand>
{
    public DeleteInvestmentCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
