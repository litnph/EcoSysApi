using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Sources.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.GetSources;

/// <summary>Reads finance sources visible to the caller for the requested module.</summary>
public sealed class GetSourcesQueryHandler : IRequestHandler<GetSourcesQuery, GetSourcesResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetSourcesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Returns all sources for the module ordered by <see cref="Domain.Entities.FinSource.SortOrder"/> then name.</summary>
    /// <inheritdoc cref="IRequestHandler{GetSourcesQuery, GetSourcesResponse}.Handle" />
    public async Task<GetSourcesResponse> Handle(GetSourcesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
        var rows = await _db.FinSources
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var installmentBySource = await SourceInstallmentMetrics
            .GetRemainingBySourceIdAsync(_db, cancellationToken)
            .ConfigureAwait(false);

        var dtos = rows
            .Select(s => FinSourceDtoMapper.ToDto(
                s,
                installmentBySource.GetValueOrDefault(s.Id, 0)))
            .ToList();
        return new GetSourcesResponse(dtos);
    }
}
