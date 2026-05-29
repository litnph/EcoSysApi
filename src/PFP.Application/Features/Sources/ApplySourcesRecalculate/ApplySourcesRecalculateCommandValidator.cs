using FluentValidation;

namespace PFP.Application.Features.Sources.ApplySourcesRecalculate;

public sealed class ApplySourcesRecalculateCommandValidator
    : AbstractValidator<ApplySourcesRecalculateCommand>
{
    public ApplySourcesRecalculateCommandValidator()
    {
        RuleFor(x => x.SourceIds).NotEmpty();
        RuleForEach(x => x.SourceIds).NotEmpty();
    }
}
