using MediatR;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Members.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Members.UpdateMember;

public sealed record UpdateMemberCommand(
    Guid MemberId,
    string FullName,
    UserRole Role,
    bool IsActive,
    string? NewPassword) : IRequest<UpdateMemberResponse>, IAuthorizeRequest
{
    public bool RequireAdmin => true;
}
