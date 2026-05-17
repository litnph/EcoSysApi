using FluentValidation;

namespace PFP.Application.Features.Auth.ForgotPassword;

/// <summary>FluentValidation rules for <see cref="ForgotPasswordCommand"/>.</summary>
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    /// <summary>Registers field rules.</summary>
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
    }
}
