using FluentValidation;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Tags.AddTagToEntity;

public sealed class AddTagToEntityCommandValidator : AbstractValidator<AddTagToEntityCommand>
{
    public AddTagToEntityCommandValidator()
    {
        RuleFor(x => x.TagId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.EntityType)
            .Must(t => string.Equals(t.Trim(), nameof(FinTransaction), StringComparison.Ordinal))
            .WithMessage("Tagging this entity kind is not supported yet.");
    }
}
