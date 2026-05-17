using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Gdpr.DeleteAccount;

/// <summary>Validates <see cref="RequestAccountDeletionCommand"/>.</summary>
public sealed class RequestAccountDeletionCommandValidator : AbstractValidator<RequestAccountDeletionCommand>
{
    /// <summary>Creates the validator.</summary>
    public RequestAccountDeletionCommandValidator(IApplicationDbContext db, ICurrentUserService current)
    {
        RuleFor(x => x.Reason).MaximumLength(2048).When(x => x.Reason is not null);

        RuleFor(_ => current.UserId)
            .NotNull()
            .WithMessage("Authentication is required.")
            .MustAsync(
                async (uid, ct) =>
                    !await db.UserDeletionRequests.AnyAsync(
                        r => r.UserId == uid!.Value
                             && (r.Status == DeletionRequestStatus.Pending
                                 || r.Status == DeletionRequestStatus.Confirmed),
                        ct))
            .WithMessage("You already have a pending or confirmed account deletion request.");
    }
}
