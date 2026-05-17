namespace PFP.Domain.Enums;

/// <summary>
/// Recognised <c>SPACES.type</c> values, used to colour the UX and to gate certain features
/// (e.g. invitations only make sense for non-personal spaces).
/// <para>
/// On registration the bootstrap flow always provisions a <see cref="Personal"/> root space
/// inside the auto-created personal organization.
/// </para>
/// </summary>
public enum SpaceType
{
    /// <summary><c>personal</c> — single-user space inside the auto-created personal organization.</summary>
    Personal = 1,

    /// <summary><c>family</c> — shared household space; multiple members with mixed roles.</summary>
    Family = 2,

    /// <summary><c>business</c> — organisational/business space (department, team, project, …).</summary>
    Business = 3,
}
