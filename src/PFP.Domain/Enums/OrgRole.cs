namespace PFP.Domain.Enums;

/// <summary>
/// Role of an <c>ORG_MEMBERS</c> row — the user's permission tier inside a single organisation.
/// <para>
/// Higher numerical value means broader permission, so handlers can perform
/// minimum-role checks via <c>actual &gt;= required</c>.
/// </para>
/// </summary>
public enum OrgRole
{
    /// <summary><c>member</c> — read access plus the operations granted at space level.</summary>
    Member = 1,

    /// <summary><c>admin</c> — can manage members, spaces and module settings, but cannot delete the org.</summary>
    Admin = 2,

    /// <summary>
    /// <c>owner</c> — full control of the organisation, including deletion and ownership transfer.
    /// Each organisation has exactly one owner at a time.
    /// </summary>
    Owner = 3,
}
