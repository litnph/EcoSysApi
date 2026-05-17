using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Translations.UpdateTranslation;

/// <summary>Patches a translation value and clears cache for its tuple.</summary>
public sealed class UpdateTranslationCommandHandler : IRequestHandler<UpdateTranslationCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ITranslationService _translation;

    /// <summary>Creates the handler.</summary>
    public UpdateTranslationCommandHandler(IApplicationDbContext db, ITranslationService translation)
    {
        _db = db;
        _translation = translation;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(UpdateTranslationCommand request, CancellationToken cancellationToken)
    {
        var row = await _db.Translations
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
            throw new NotFoundException("Translation was not found.");

        row.Value = request.Value;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _translation
            .RemoveTranslationCacheAsync(
                row.LocaleCode,
                row.EntityType,
                row.EntityId,
                row.Field,
                cancellationToken)
            .ConfigureAwait(false);

        return Unit.Value;
    }
}
