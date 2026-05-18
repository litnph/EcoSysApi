using FluentValidation;

namespace PFP.Application.Features.Transactions.UpdateTransaction;

/// <summary>FluentValidation rules for <see cref="UpdateTransactionCommand"/>.</summary>
public sealed class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    /// <summary>Registers field rules.</summary>
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
