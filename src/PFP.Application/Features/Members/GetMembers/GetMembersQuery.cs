using MediatR;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Members.Common;

namespace PFP.Application.Features.Members.GetMembers;

public sealed record GetMembersQuery : IRequest<GetMembersResponse>, IAuthorizeRequest
{
    public bool RequireAdmin => true;
}
