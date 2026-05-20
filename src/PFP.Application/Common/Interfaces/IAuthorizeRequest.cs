namespace PFP.Application.Common.Interfaces;

/// <summary>
/// Optional marker on commands/queries that declare authorisation requirements
/// resolved by <see cref="Behaviors.AuthorizationBehavior"/>.
/// </summary>
public interface IAuthorizeRequest
{
    /// <summary>When <c>true</c>, an authenticated session must be present.</summary>
    bool RequireAuthenticated => true;

    /// <summary>When <c>true</c>, the current user must have <see cref="Domain.Enums.UserRole.Admin"/>.</summary>
    bool RequireAdmin => false;
}
