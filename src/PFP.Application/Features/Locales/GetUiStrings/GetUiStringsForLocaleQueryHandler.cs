using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Locales.GetUiStrings;

/// <summary>Returns all UI strings for one locale (front-end bootstrap).</summary>
public sealed class GetUiStringsForLocaleQueryHandler : IRequestHandler<GetUiStringsForLocaleQuery, IReadOnlyDictionary<string, string>>
{
    private readonly IApplicationDbContext _db;

    /// <summary>Creates the handler.</summary>
    public GetUiStringsForLocaleQueryHandler(IApplicationDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, string>> Handle(GetUiStringsForLocaleQuery request, CancellationToken cancellationToken)
    {
        var exists = await _db.Locales.AsNoTracking()
            .AnyAsync(l => l.Code == request.LocaleCode && l.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
            throw new NotFoundException("Locale was not found or is inactive.");

        var pairs = await _db.UIStrings.AsNoTracking()
            .Where(s => s.LocaleCode == request.LocaleCode)
            .Select(s => new { s.Key, s.Value })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return pairs.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
    }
}
