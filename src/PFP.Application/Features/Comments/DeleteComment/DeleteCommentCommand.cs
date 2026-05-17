using MediatR;

namespace PFP.Application.Features.Comments.DeleteComment;

public sealed record DeleteCommentCommand(Guid Id) : IRequest<Unit>;
