namespace PFP.API.Models;

/// <summary>JSON body for account deletion request.</summary>
public sealed class RequestAccountDeletionBody
{
    /// <summary>Optional user-provided reason.</summary>
    public string? Reason { get; set; }
}

/// <summary>JSON body for account deletion confirmation.</summary>
public sealed class ConfirmAccountDeletionBody
{
    /// <summary>Opaque token from the confirmation email.</summary>
    public string Token { get; set; } = string.Empty;
}
