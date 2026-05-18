using PFP.Domain.Enums;

namespace PFP.Application.Features.SpaceModules.Common;

/// <summary>Compact <c>SPACE_MODULES</c> row returned by list / toggle endpoints.</summary>
public sealed record SpaceModuleDto(
    Guid Id,
    Guid SpaceId,
    ModuleCode ModuleCode,
    bool IsEnabled,
    string? Settings,
    DateTime EnabledAt,
    DateTime? DisabledAt);
