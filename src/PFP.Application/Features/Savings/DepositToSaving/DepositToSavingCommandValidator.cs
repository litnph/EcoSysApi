using FluentValidation;

namespace PFP.Application.Features.Savings.DepositToSaving;

public sealed class DepositToSavingCommandValidator : AbstractValidator<DepositToSavingCommand>
{
    public DepositToSavingCommandValidator()
    {
        RuleFor(x => x.SavingId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
