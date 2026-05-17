using MediatR;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Translations.CreateTranslation;

/// <summary>Persists a translation and expires its cache entry.</summary>
public sealed class CreateTranslationCommandHandler : IRequestHandler<CreateTranslationCommand, CreateTranslationResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ITranslationService _translation;

    /// <summary>Creates the handler.</summary>
    public CreateTranslationCommandHandler(IApplicationDbContext db, ITranslationService translation)
    {
        _db = db;
        _translation = translation;
    }

    /// <inheritdoc/>
    public async Task<CreateTranslationResponse> Handle(CreateTranslationCommand request, CancellationToken cancellationToken)
    {
        var entity = new Translation
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Field = request.Field,
            LocaleCode = request.LocaleCode,
            Value = request.Value,
        };

        _db.Translations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _translation
            .RemoveTranslationCacheAsync(
                request.LocaleCode,
                request.EntityType,
                request.EntityId,
                request.Field,
                cancellationToken)
            .ConfigureAwait(false);

        return new CreateTranslationResponse(entity.Id);
    }
}
