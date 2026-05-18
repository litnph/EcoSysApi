using MediatR;
using PFP.Application.Features.Organizations.Common;

namespace PFP.Application.Features.Organizations.GetMyOrganizations;

/// <summary>Lists every active organisation the current user belongs to.</summary>
public sealed record GetMyOrganizationsQuery() : IRequest<GetMyOrganizationsResponse>;

/// <summary>Response envelope.</summary>
public sealed record GetMyOrganizationsResponse(IReadOnlyList<OrganizationListItemDto> Items);
