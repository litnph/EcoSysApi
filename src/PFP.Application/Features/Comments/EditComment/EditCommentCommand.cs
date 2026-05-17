using MediatR;

namespace PFP.Application.Features.Comments.EditComment;

public sealed record EditCommentCommand(Guid Id, string Content) : IRequest<Unit>;
