using FluentValidation;

namespace PFP.Application.Features.Investments.CreateInvestment;

public sealed class CreateInvestmentCommandValidator : AbstractValidator<CreateInvestmentCommand>
{
    public CreateInvestmentCommandValidator()
    {
RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Currency).Length(3).When(x => !string.IsNullOrWhiteSpace(x.Currency));
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
