namespace PFP.Domain.Enums;

/// <summary>
/// Identifier of a pluggable platform module. Stored in <c>SPACE_MODULES.module_code</c>
/// (and referenced in places such as <c>USER_NOTIFICATION_PREFS.module_code</c>).
/// <para>
/// A module is enabled per-space; finance-domain entities (<c>FIN_*</c>) reference a space
/// indirectly through <c>smodule_id</c> → <c>SPACE_MODULES</c>, never through <c>SPACES</c> directly.
/// </para>
/// <para>
/// Only modules implemented by the current MVP are listed. New modules MUST be added here
/// (and to the matching DB seed) before being referenced anywhere else.
/// </para>
/// </summary>
public enum ModuleCode
{
    /// <summary>
    /// <c>finance</c> — personal-finance module: sources, transactions, billing cycles,
    /// installment plans, debts, savings, investments, monthly periods.
    /// </summary>
    Finance = 1,
}
