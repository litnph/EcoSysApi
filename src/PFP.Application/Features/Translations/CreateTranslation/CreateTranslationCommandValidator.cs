using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Translations.CreateTranslation;

/// <summary>Validates <see cref="CreateTranslationCommand"/>.</summary>
public sealed class CreateTranslationCommandValidator : AbstractValidator<CreateTranslationCommand>
{
    /// <summary>Creates the validator.</summary>
    public CreateTranslationCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.EntityId).NotEmpty();

        RuleFor(x => x.Field)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.LocaleCode)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(8192);

        RuleFor(x => x.LocaleCode)
            .MustAsync(
                async (code, ct) =>
                    await db.Locales.AsNoTracking().AnyAsync(l => l.Code == code && l.IsActive, ct))
            .WithMessage("Locale is unknown or inactive.");

        RuleFor(x => x)
            .MustAsync(
                async (cmd, ct) =>
                    !await db.Translations.AnyAsync(
                        t => t.EntityType == cmd.EntityType
                             && t.EntityId == cmd.EntityId
                             && t.Field == cmd.Field
                             && t.LocaleCode == cmd.LocaleCode,
                        ct))
            .WithMessage("A translation already exists for this entity field and locale.");
    }
}
