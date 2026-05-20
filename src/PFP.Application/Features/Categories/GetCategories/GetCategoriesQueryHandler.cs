using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Categories.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.GetCategories;

/// <summary>Builds a nested category tree for the requested module + kind.</summary>
public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetCategoriesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Loads categories and materialises a tree.</summary>
    /// <inheritdoc cref="IRequestHandler{GetCategoriesQuery, GetCategoriesResponse}.Handle" />
    public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var flat = await _db.FinCategories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var roots = CategoryDtoMapper.BuildTree(flat);
        return new GetCategoriesResponse(roots);
    }
}
