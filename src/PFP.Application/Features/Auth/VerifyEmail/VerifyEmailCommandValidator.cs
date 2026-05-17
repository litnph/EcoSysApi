using FluentValidation;

namespace PFP.Application.Features.Auth.VerifyEmail;

/// <summary>FluentValidation rules for <see cref="VerifyEmailCommand"/>.</summary>
public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    /// <summary>Registers field rules.</summary>
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
