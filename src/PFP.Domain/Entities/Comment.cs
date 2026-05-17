namespace PFP.Domain.Entities;

/// <summary>Threaded textual note attached polymorphically to a domain aggregate (<c>COMMENTS</c>).</summary>
public sealed class Comment : SoftDeletableEntity
{
    public string ModuleCode { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    /// <summary>Parent reply thread; <c>null</c> for top-level.</summary>
    public Guid? ParentId { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>Author (<c>USERS.Id</c>).</summary>
    public Guid AuthorId { get; set; }

    public bool IsEdited { get; set; }

    /// <summary>UTC time of last author edit (<c>null</c> if never).</summary>
    public DateTime? EditedAt { get; set; }

    // ---- Navigation ----

    public User Author { get; set; } = null!;

    public Comment? Parent { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
