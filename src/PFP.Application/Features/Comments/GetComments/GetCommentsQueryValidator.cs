using FluentValidation;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Comments.GetComments;

public sealed class GetCommentsQueryValidator : AbstractValidator<GetCommentsQuery>
{
    public GetCommentsQueryValidator()
    {
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.ModuleCode).MaximumLength(50);
        RuleFor(x => x.EntityType)
            .Must(t => string.Equals(t.Trim(), nameof(FinTransaction), StringComparison.Ordinal))
            .WithMessage("Listing comments for this entity kind is not supported yet.");
    }
}
