using FluentValidation;

namespace PFP.Application.Features.Comments.DeleteComment;

public sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
