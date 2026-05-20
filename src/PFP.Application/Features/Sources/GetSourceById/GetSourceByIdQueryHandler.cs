using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Sources.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.GetSourceById;

/// <summary>Returns one <see cref="Domain.Entities.FinSource"/> when the caller can read its parent module.</summary>
public sealed class GetSourceByIdQueryHandler : IRequestHandler<GetSourceByIdQuery, GetSourceByIdResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetSourceByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Returns one source including its persisted balance (whole currency units in the DTO).</summary>
    /// <inheritdoc cref="IRequestHandler{GetSourceByIdQuery, GetSourceByIdResponse}.Handle" />
    public async Task<GetSourceByIdResponse> Handle(GetSourceByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinSources
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Finance source was not found.");
return new GetSourceByIdResponse(FinSourceDtoMapper.ToDto(entity));
    }
}
