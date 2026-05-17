using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Translations.GetEntityTranslations;

/// <summary>Reads translation rows without resolving fallbacks.</summary>
public sealed class GetEntityTranslationsQueryHandler
    : IRequestHandler<GetEntityTranslationsQuery, IReadOnlyList<EntityTranslationAdminDto>>
{
    private readonly IApplicationDbContext _db;

    /// <summary>Creates the handler.</summary>
    public GetEntityTranslationsQueryHandler(IApplicationDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EntityTranslationAdminDto>> Handle(
        GetEntityTranslationsQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _db.Translations.AsNoTracking()
            .Where(t => t.EntityType == request.EntityType && t.EntityId == request.EntityId)
            .OrderBy(t => t.Field)
            .ThenBy(t => t.LocaleCode)
            .Select(t => new EntityTranslationAdminDto(
                t.Id,
                t.EntityType,
                t.EntityId,
                t.Field,
                t.LocaleCode,
                t.Value,
                t.CreatedAt,
                t.UpdatedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows;
    }
}
