namespace PFP.Domain.Enums;

/// <summary>
/// Role of a <c>SPACE_MEMBERS</c> row — the user's permission tier inside a single space.
/// <para>
/// Space membership can be inherited from a parent space (<c>SPACE_MEMBERS.inherited = true</c>);
/// inherited rows still carry an explicit role so that authorisation never has to walk the tree at request time.
/// Higher numerical value means broader permission.
/// </para>
/// </summary>
public enum SpaceRole
{
    /// <summary><c>viewer</c> — read-only access to data inside the space.</summary>
    Viewer = 1,

    /// <summary><c>editor</c> — can create / update / soft-delete records owned by the space.</summary>
    Editor = 2,

    /// <summary>
    /// <c>manager</c> — full control of the space (settings, modules, members, child spaces).
    /// Cannot affect sibling spaces or the parent organisation.
    /// </summary>
    Manager = 3,
}
