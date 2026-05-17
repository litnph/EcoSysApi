using FluentValidation;

namespace PFP.Application.Features.Auth.Logout;

/// <summary>Validator placeholder — <see cref="LogoutCommand"/> carries no payload.</summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    /// <summary>Creates the validator (no rules; authorisation is enforced at the API layer).</summary>
    public LogoutCommandValidator()
    {
        // Intentionally empty — logout is driven purely by the authenticated principal.
    }
}
