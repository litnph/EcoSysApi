using FluentValidation;

namespace PFP.Application.Features.DebtRecords.RecordDebtPayment;

/// <summary>FluentValidation rules for <see cref="RecordDebtPaymentCommand"/>.</summary>
public sealed class RecordDebtPaymentCommandValidator : AbstractValidator<RecordDebtPaymentCommand>
{
    /// <summary>Registers field rules.</summary>
    public RecordDebtPaymentCommandValidator()
    {
        RuleFor(x => x.DebtRecordId).NotEmpty();
        RuleFor(x => x.SourceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
