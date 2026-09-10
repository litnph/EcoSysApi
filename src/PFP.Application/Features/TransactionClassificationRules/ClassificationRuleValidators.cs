using FluentValidation;

namespace PFP.Application.Features.TransactionClassificationRules;

public sealed class CreateClassificationRuleCommandValidator : AbstractValidator<CreateClassificationRuleCommand>
{
    public CreateClassificationRuleCommandValidator()
    {
        RuleFor(x => x.Keyword).NotEmpty().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(120);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public sealed class UpdateClassificationRuleCommandValidator : AbstractValidator<UpdateClassificationRuleCommand>
{
    public UpdateClassificationRuleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Keyword).NotEmpty().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(120);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public sealed class MatchTransactionContentsCommandValidator : AbstractValidator<MatchTransactionContentsCommand>
{
    public MatchTransactionContentsCommandValidator()
    {
        RuleFor(x => x.Items).NotNull().Must(x => x.Count is > 0 and <= 100);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
            item.RuleFor(x => x.Content).MaximumLength(512);
        });
    }
}
