using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.SpaceMembers.InviteSpaceMember;

public sealed record InviteSpaceMemberCommand(
    Guid SpaceId,
    Guid UserId,
    SpaceRole Role) : IRequest<InviteSpaceMemberResponse>;
