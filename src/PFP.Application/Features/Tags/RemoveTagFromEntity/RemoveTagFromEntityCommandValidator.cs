using FluentValidation;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Tags.RemoveTagFromEntity;

public sealed class RemoveTagFromEntityCommandValidator : AbstractValidator<RemoveTagFromEntityCommand>
{
    public RemoveTagFromEntityCommandValidator()
    {
        RuleFor(x => x.TagId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.EntityType)
            .Must(t => string.Equals(t.Trim(), nameof(FinTransaction), StringComparison.Ordinal))
            .WithMessage("Untagging this entity kind is not supported yet.");
    }
}
