using MediatR;

namespace PFP.Application.Features.Organizations.LeaveOrganization;

/// <summary>Lets the current user voluntarily leave a non-personal organisation.</summary>
public sealed record LeaveOrganizationCommand(Guid OrganizationId) : IRequest<LeaveOrganizationResponse>;

/// <summary>Response acknowledging the leave operation.</summary>
public sealed record LeaveOrganizationResponse(Guid OrganizationId);
