using MediatR;
using PFP.Application.Features.SpaceModules.Common;

namespace PFP.Application.Features.SpaceModules.GetSpaceModules;

/// <summary>Lists every module activation row attached to one space.</summary>
public sealed record GetSpaceModulesQuery(Guid SpaceId) : IRequest<GetSpaceModulesResponse>;

/// <summary>Response envelope.</summary>
public sealed record GetSpaceModulesResponse(IReadOnlyList<SpaceModuleDto> Modules);
