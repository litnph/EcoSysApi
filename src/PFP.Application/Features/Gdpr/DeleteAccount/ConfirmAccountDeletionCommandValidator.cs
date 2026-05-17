using FluentValidation;

namespace PFP.Application.Features.Gdpr.DeleteAccount;

/// <summary>Validates <see cref="ConfirmAccountDeletionCommand"/>.</summary>
public sealed class ConfirmAccountDeletionCommandValidator : AbstractValidator<ConfirmAccountDeletionCommand>
{
    /// <summary>Creates the validator.</summary>
    public ConfirmAccountDeletionCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
