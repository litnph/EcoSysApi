using MediatR;
using PFP.Application.Features.SpaceModules.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.SpaceModules.ToggleSpaceModule;

/// <summary>
/// Enables or disables a module on a space (idempotent upsert). When disabling, finance rows
/// already attached to the module remain intact — only new writes are blocked.
/// </summary>
public sealed record ToggleSpaceModuleCommand(
    Guid SpaceId,
    ModuleCode ModuleCode,
    bool Enable) : IRequest<ToggleSpaceModuleResponse>;

/// <summary>Response wrapping the persisted activation row.</summary>
public sealed record ToggleSpaceModuleResponse(SpaceModuleDto Module);
