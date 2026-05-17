using FluentValidation;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Comments.AddComment;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.ModuleCode).MaximumLength(50);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.EntityType)
            .Must(t => string.Equals(t.Trim(), nameof(FinTransaction), StringComparison.Ordinal))
            .WithMessage("Commenting on this entity kind is not supported yet.");
    }
}
