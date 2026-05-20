using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Members.Common;

namespace PFP.Application.Features.Members.GetMembers;

public sealed class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, GetMembersResponse>
{
    private readonly IApplicationDbContext _db;

    public GetMembersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<GetMembersResponse> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var items = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new MemberListItemDto(
                u.Id,
                u.Email,
                u.FullName,
                u.Role,
                u.IsActive,
                u.LastLoginAt,
                u.CreatedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetMembersResponse(items);
    }
}
