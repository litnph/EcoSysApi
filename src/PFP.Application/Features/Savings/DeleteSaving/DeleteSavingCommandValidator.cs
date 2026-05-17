using FluentValidation;

namespace PFP.Application.Features.Savings.DeleteSaving;

public sealed class DeleteSavingCommandValidator : AbstractValidator<DeleteSavingCommand>
{
    public DeleteSavingCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
