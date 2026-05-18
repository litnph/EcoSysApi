using MediatR;
using PFP.Application.Features.Organizations.Common;

namespace PFP.Application.Features.Organizations.GetOrganizationDetail;

/// <summary>Returns the full detail of one organisation the caller belongs to.</summary>
public sealed record GetOrganizationDetailQuery(Guid OrganizationId) : IRequest<GetOrganizationDetailResponse>;

/// <summary>Response wrapper.</summary>
public sealed record GetOrganizationDetailResponse(OrganizationDetailDto Organization);
