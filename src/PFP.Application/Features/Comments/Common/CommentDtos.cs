namespace PFP.Application.Features.Comments.Common;

/// <summary>Recursive comment projection for threaded UI.</summary>
public sealed record CommentTreeNodeDto(
    Guid Id,
    Guid? ParentId,
    string Content,
    Guid AuthorId,
    string AuthorFullName,
    DateTime CreatedAtUtc,
    bool IsEdited,
    DateTime? EditedAtUtc,
    IReadOnlyList<CommentTreeNodeDto> Replies);

public sealed record GetCommentsResponse(IReadOnlyList<CommentTreeNodeDto> Threads);
