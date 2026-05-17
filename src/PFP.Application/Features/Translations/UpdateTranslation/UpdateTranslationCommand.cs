using FluentValidation;
using MediatR;

namespace PFP.Application.Features.Translations.UpdateTranslation;

/// <summary>Updates the text of an existing translation.</summary>
public sealed record UpdateTranslationCommand(Guid Id, string Value) : IRequest<Unit>;

/// <summary>Validates <see cref="UpdateTranslationCommand"/>.</summary>
public sealed class UpdateTranslationCommandValidator : AbstractValidator<UpdateTranslationCommand>
{
    /// <summary>Creates the validator.</summary>
    public UpdateTranslationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(8192);
    }
}
