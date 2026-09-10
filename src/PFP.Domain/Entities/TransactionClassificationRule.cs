namespace PFP.Domain.Entities;

/// <summary>A user-owned reusable rule for classifying imported transactions.</summary>
public sealed class TransactionClassificationRule : SoftDeletableEntity
{
    public Guid UserId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string NormalizedKeyword { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid? TagId { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public FinCategory Category { get; set; } = null!;
    public Tag? Tag { get; set; }
}
