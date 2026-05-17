using FluentValidation;

namespace PFP.Application.Features.Auth.Google;

/// <summary>FluentValidation rules for <see cref="GoogleLoginCommand"/>.</summary>
public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    /// <summary>Registers field rules.</summary>
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.GoogleSubject)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(255);
    }
}
