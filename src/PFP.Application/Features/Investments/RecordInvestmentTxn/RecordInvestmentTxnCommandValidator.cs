using FluentValidation;

namespace PFP.Application.Features.Investments.RecordInvestmentTxn;

public sealed class RecordInvestmentTxnCommandValidator : AbstractValidator<RecordInvestmentTxnCommand>
{
    public RecordInvestmentTxnCommandValidator()
    {
        RuleFor(x => x.InvestmentId).NotEmpty();
        RuleFor(x => x.TxnType).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
