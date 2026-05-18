using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Users.Common;

namespace PFP.Application.Features.Users.GetMe;

/// <summary>Joins <c>USERS</c> + <c>USER_PROFILES</c> + active avatar + personal org for one round trip.</summary>
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
            join o in _db.Organizations.AsNoTracking().Where(x => x.IsPersonal) on u.Id equals o.OwnerId into oo
            from personalOrg in oo.DefaultIfEmpty()
            select new
            {
                u.Id,
                u.Email,
                u.FullName,
                u.IsEmailVerified,
                u.LastLoginAt,
                LanguageCode = prof != null ? prof.LanguageCode : "vi",
                Timezone = prof != null ? prof.Timezone : "Asia/Ho_Chi_Minh",
                Theme = prof != null ? prof.Theme : "system",
                PersonalOrgId = personalOrg != null ? personalOrg.Id : Guid.Empty,
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
            throw new NotFoundException("User was not found.");

        var avatarUrl = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.AvatarUploads)
            .Where(a => a.IsActive)
            .Select(a => a.StorageUrl)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var dto = new UserMeDto(
            data.Id,
            data.Email,
            data.FullName,
            data.IsEmailVerified,
            data.LastLoginAt,
            data.LanguageCode,
            data.Timezone,
            data.Theme,
            avatarUrl,
            data.PersonalOrgId);

        return new GetMeResponse(dto);
    }
}
