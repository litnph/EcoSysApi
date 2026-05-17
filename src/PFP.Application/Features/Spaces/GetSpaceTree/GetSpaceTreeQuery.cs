using MediatR;

namespace PFP.Application.Features.Spaces.GetSpaceTree;

/// <summary>Returns nested spaces for one organisation joined with finance-module activation flags.</summary>
public sealed record GetSpaceTreeQuery(Guid OrgId) : IRequest<GetSpaceTreeResponse>;
