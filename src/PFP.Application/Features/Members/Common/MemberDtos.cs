using PFP.Domain.Enums;

namespace PFP.Application.Features.Members.Common;

public sealed record MemberListItemDto(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public sealed record GetMembersResponse(IReadOnlyList<MemberListItemDto> Items);

public sealed record MemberDetailDto(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public sealed record CreateMemberResponse(MemberDetailDto Member);

public sealed record UpdateMemberResponse(MemberDetailDto Member);
