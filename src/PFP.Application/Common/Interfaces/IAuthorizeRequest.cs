using PFP.Domain.Enums;

namespace PFP.Application.Common.Interfaces;

/// <summary>
/// Optional marker on commands/queries that declare an explicit authorisation requirement
/// resolved by the <c>AuthorizationBehavior</c> in the MediatR pipeline (spec §2.3 / §6.2).
/// <para>
/// Implementing the interface lets the handler skip ad-hoc checks: the pipeline reads the
/// declared requirement before dispatching to the handler and throws <c>ForbiddenException</c>
/// when the current principal does not satisfy it.
/// </para>
/// <para>
/// Both <see cref="RequiredOrgId"/> + <see cref="MinimumOrgRole"/> and
/// <see cref="RequiredSpaceId"/> + <see cref="MinimumSpaceRole"/> and
/// <see cref="RequiredSpaceModuleId"/> + <see cref="MinimumSpaceModuleRole"/> are optional
/// and orthogonal — set whichever subset applies to the request. <see cref="RequireAuthenticated"/>
/// defaults to <c>true</c>; commands that should accept anonymous calls override it explicitly.
/// </para>
/// </summary>
public interface IAuthorizeRequest
{
    /// <summary>When <c>true</c>, an authenticated session must be present in <c>ICurrentUserService</c>.</summary>
    bool RequireAuthenticated => true;

    /// <summary>Organisation the request operates inside; <c>null</c> when not org-scoped.</summary>
    Guid? RequiredOrgId => null;

    /// <summary>Minimum org-level role required for the request.</summary>
    OrgRole MinimumOrgRole => OrgRole.Member;

    /// <summary>Space the request operates inside; <c>null</c> when not space-scoped.</summary>
    Guid? RequiredSpaceId => null;

    /// <summary>Minimum space-level role required for the request.</summary>
    SpaceRole MinimumSpaceRole => SpaceRole.Viewer;

    /// <summary>SpaceModule the request operates against; <c>null</c> when not module-scoped.</summary>
    Guid? RequiredSpaceModuleId => null;

    /// <summary>Minimum space-module role required for the request.</summary>
    SpaceRole MinimumSpaceModuleRole => SpaceRole.Viewer;
}
