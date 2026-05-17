using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Locales.GetLocales;

/// <summary>Loads locales ordered default-first.</summary>
public sealed class GetLocalesQueryHandler : IRequestHandler<GetLocalesQuery, IReadOnlyList<LocaleListItemDto>>
{
    private readonly IApplicationDbContext _db;

    /// <summary>Creates the handler.</summary>
    public GetLocalesQueryHandler(IApplicationDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<LocaleListItemDto>> Handle(GetLocalesQuery request, CancellationToken cancellationToken)
    {
        var rows = await _db.Locales.AsNoTracking()
            .OrderByDescending(l => l.IsDefault)
            .ThenBy(l => l.Code)
            .Select(l => new LocaleListItemDto(
                l.Code,
                l.Name,
                l.EnglishName,
                l.Direction,
                l.IsDefault,
                l.IsActive))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows;
    }
}
