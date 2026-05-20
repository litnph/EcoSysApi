using MediatR;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Members.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Members.CreateMember;

public sealed record CreateMemberCommand(
    string Email,
    string Password,
    string FullName,
    UserRole Role) : IRequest<CreateMemberResponse>, IAuthorizeRequest
{
    public bool RequireAdmin => true;
}
