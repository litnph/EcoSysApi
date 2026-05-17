using FluentValidation;

namespace PFP.Application.Features.Auth.RefreshToken;

/// <summary>FluentValidation rules for <see cref="RefreshTokenCommand"/>.</summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>Registers field rules.</summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
