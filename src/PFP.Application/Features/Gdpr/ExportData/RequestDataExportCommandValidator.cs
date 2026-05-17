using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Gdpr.ExportData;

/// <summary>Validates <see cref="RequestDataExportCommand"/>.</summary>
public sealed class RequestDataExportCommandValidator : AbstractValidator<RequestDataExportCommand>
{
    /// <summary>Creates the validator.</summary>
    public RequestDataExportCommandValidator(IApplicationDbContext db, ICurrentUserService current)
    {
        RuleFor(_ => current.UserId)
            .NotNull()
            .WithMessage("Authentication is required.")
            .MustAsync(
                async (uid, ct) =>
                    !await db.UserDataExports.AnyAsync(
                        e => e.UserId == uid!.Value
                             && (e.Status == DataExportStatus.Pending || e.Status == DataExportStatus.Processing),
                        ct))
            .WithMessage("You already have a data export in progress. Please wait until it finishes.");
    }
}
