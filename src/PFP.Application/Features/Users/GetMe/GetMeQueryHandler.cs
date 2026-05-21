using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Users.Common;

namespace PFP.Application.Features.Users.GetMe;

/// <summary>Joins <c>users</c> + <c>user_profiles</c> for one round trip.</summary>
public sealed class GetMeQueryHandler : IRequestHandler<GetMeQuery, GetMeResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetMeQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetMeResponse> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var data = await (
            from u in _db.Users.AsNoTracking()
            where u.Id == userId
            join p in _db.UserProfiles.AsNoTracking() on u.Id equals p.UserId into pp
            from prof in pp.DefaultIfEmpty()
            select new
            {
                u.Id,
                u.Email,
                u.FullName,
                u.Role,
                u.LastLoginAt,
                LanguageCode = prof != null ? prof.LanguageCode : "vi",
                Timezone = prof != null ? prof.Timezone : "Asia/Ho_Chi_Minh",
                Theme = prof != null ? prof.Theme : "system",
                AvatarUrl = prof != null ? prof.AvatarUrl : null,
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
            throw new NotFoundException("User was not found.");

        var dto = new UserMeDto(
            data.Id,
            data.Email,
            data.FullName,
            data.Role,
            data.LastLoginAt,
            data.LanguageCode,
            data.Timezone,
            data.Theme,
            data.AvatarUrl);

        return new GetMeResponse(dto);
    }
}
