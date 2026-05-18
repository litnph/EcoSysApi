using MediatR;

namespace PFP.Application.Features.Organizations.DeleteOrganization;

/// <summary>
/// Deletes an organisation. Only the <see cref="Domain.Enums.OrgRole.Owner"/> may invoke this,
/// and personal organisations cannot be deleted via this endpoint (they are removed by the
/// GDPR account-deletion flow). Soft-deletion cascades to the org's spaces and modules; user
/// rows remain intact.
/// </summary>
public sealed record DeleteOrganizationCommand(Guid OrganizationId) : IRequest<DeleteOrganizationResponse>;

/// <summary>Response acknowledging the soft delete.</summary>
public sealed record DeleteOrganizationResponse(Guid OrganizationId);
