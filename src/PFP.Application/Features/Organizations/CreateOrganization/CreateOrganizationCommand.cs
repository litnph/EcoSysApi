using MediatR;
using PFP.Application.Features.Organizations.Common;

namespace PFP.Application.Features.Organizations.CreateOrganization;

/// <summary>
/// Creates a non-personal organisation (family / business / group). The caller becomes the
/// owner (<c>org_role = Owner</c>) and a root space "General" is auto-provisioned with the
/// finance module enabled (parity with the personal-org bootstrap in spec §4.1).
/// </summary>
public sealed record CreateOrganizationCommand(
    string Name,
    string Slug,
    string? DefaultCurrency,
    string? Description) : IRequest<CreateOrganizationResponse>;

/// <summary>Wrapper around the freshly created organisation detail.</summary>
public sealed record CreateOrganizationResponse(OrganizationDetailDto Organization);
