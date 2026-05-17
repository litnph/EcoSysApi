using FluentValidation;

namespace PFP.Application.Features.Auth.Login;

/// <summary>FluentValidation rules for <see cref="LoginCommand"/>.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Registers field rules for login.</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}
