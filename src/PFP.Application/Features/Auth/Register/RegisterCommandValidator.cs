using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Auth;

namespace PFP.Application.Features.Auth.Register;

/// <summary>FluentValidation rules for <see cref="RegisterCommand"/>.</summary>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>Registers async DB rules (unique email) and password strength checks.</summary>
    public RegisterCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(256)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

        RuleFor(x => x.Email)
            .MustAsync(
                async (email, ct) =>
                    !await db.Users.AnyAsync(
                        u => u.Email == AuthEmailNormalizer.Normalize(email),
                        ct)
                    .ConfigureAwait(false))
            .WithMessage("This email address is already registered.");
    }
}
