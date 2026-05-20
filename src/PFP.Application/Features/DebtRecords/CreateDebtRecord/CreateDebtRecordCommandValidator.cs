using FluentValidation;

namespace PFP.Application.Features.DebtRecords.CreateDebtRecord;

/// <summary>FluentValidation rules for <see cref="CreateDebtRecordCommand"/>.</summary>
public sealed class CreateDebtRecordCommandValidator : AbstractValidator<CreateDebtRecordCommand>
{
    /// <summary>Registers field rules.</summary>
    public CreateDebtRecordCommandValidator()
    {
RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.PersonName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PersonContact).MaximumLength(200).When(x => x.PersonContact is not null);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).MaximumLength(3).When(x => !string.IsNullOrWhiteSpace(x.Currency));
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
