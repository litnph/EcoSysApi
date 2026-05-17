using FluentValidation;

namespace PFP.Application.Features.Sources.DeleteSource;

/// <summary>FluentValidation rules for <see cref="DeleteSourceCommand"/>.</summary>
public sealed class DeleteSourceCommandValidator : AbstractValidator<DeleteSourceCommand>
{
    /// <summary>Registers field rules for source deletion.</summary>
    public DeleteSourceCommandValidator() =>
        RuleFor(x => x.Id).NotEmpty();
}
