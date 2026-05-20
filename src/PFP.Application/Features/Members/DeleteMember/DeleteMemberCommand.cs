using MediatR;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Members.DeleteMember;

public sealed record DeleteMemberCommand(Guid MemberId) : IRequest<Unit>, IAuthorizeRequest
{
    public bool RequireAdmin => true;
}
