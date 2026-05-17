using FluentValidation;

namespace PFP.Application.Features.Savings.UpdateSaving;

public sealed class UpdateSavingCommandValidator : AbstractValidator<UpdateSavingCommand>
{
    public UpdateSavingCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SourceId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetAmount).GreaterThan(0).When(x => x.TargetAmount.HasValue);
        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
