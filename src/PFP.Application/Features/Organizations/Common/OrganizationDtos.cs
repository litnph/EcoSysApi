using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.Common;

/// <summary>Compact organisation row for list endpoints.</summary>
public sealed record OrganizationListItemDto(
    Guid Id,
    string Slug,
    string Name,
    bool IsPersonal,
    string DefaultCurrency,
    OrgRole MyRole,
    int MemberCount,
    DateTime CreatedAt);

/// <summary>Full organisation detail.</summary>
public sealed record OrganizationDetailDto(
    Guid Id,
    string Slug,
    string Name,
    bool IsPersonal,
    Guid OwnerId,
    string DefaultCurrency,
    string? Description,
    OrgRole MyRole,
    int MemberCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int Version);

/// <summary>Compact org-member row.</summary>
public sealed record OrgMemberDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    OrgRole Role,
    bool IsActive,
    DateTime? JoinedAt,
    DateTime? LeftAt,
    Guid? InvitedBy);
