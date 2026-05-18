using MediatR;
using PFP.Application.Features.Organizations.Common;

namespace PFP.Application.Features.Organizations.UpdateOrganization;

/// <summary>
/// Updates the editable metadata of an organisation (name, default currency, description).
/// Slug, ownership transfer and personal-flag changes are intentionally out of scope —
/// each of those has a dedicated flow.
/// </summary>
public sealed record UpdateOrganizationCommand(
    Guid OrganizationId,
    string Name,
    string? DefaultCurrency,
    string? Description) : IRequest<UpdateOrganizationResponse>;

/// <summary>Response wrapper.</summary>
public sealed record UpdateOrganizationResponse(OrganizationDetailDto Organization);
