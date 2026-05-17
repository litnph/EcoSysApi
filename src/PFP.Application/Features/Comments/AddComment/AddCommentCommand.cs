using MediatR;

namespace PFP.Application.Features.Comments.AddComment;

public sealed record AddCommentResponse(Guid Id);

public sealed record AddCommentCommand(
    string ModuleCode,
    string EntityType,
    Guid EntityId,
    Guid? ParentId,
    string Content) : IRequest<AddCommentResponse>;
