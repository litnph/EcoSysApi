using FluentValidation;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.CreateSource;

/// <summary>FluentValidation rules for <see cref="CreateSourceCommand"/>.</summary>
public sealed class CreateSourceCommandValidator : AbstractValidator<CreateSourceCommand>
{
    /// <summary>Registers field rules for source creation.</summary>
    public CreateSourceCommandValidator()
    {
        RuleFor(x => x.SmoduleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Currency)
            .MaximumLength(3)
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));
        RuleFor(x => x.Icon).MaximumLength(64).When(x => x.Icon is not null);
        RuleFor(x => x.Color).MaximumLength(16).When(x => x.Color is not null);

        When(x => x.Type == SourceType.CreditCard, () =>
        {
            RuleFor(x => x.CreditLimit).NotNull().GreaterThan(0);
            RuleFor(x => x.StatementDay).NotNull().InclusiveBetween(1, 31);
            RuleFor(x => x.PaymentDueDay).NotNull().GreaterThan(0);
            RuleFor(x => x.MinInstallmentAmt).GreaterThan(0).When(x => x.MinInstallmentAmt.HasValue);
        });

        When(x => x.Type != SourceType.CreditCard, () =>
        {
            RuleFor(x => x.CreditLimit).Null();
            RuleFor(x => x.StatementDay).Null();
            RuleFor(x => x.PaymentDueDay).Null();
            RuleFor(x => x.MinInstallmentAmt).Null();
        });
    }
}
