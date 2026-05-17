using FluentValidation;

namespace PFP.Application.Features.Investments.UpdateInvestment;

public sealed class UpdateInvestmentCommandValidator : AbstractValidator<UpdateInvestmentCommand>
{
    public UpdateInvestmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
