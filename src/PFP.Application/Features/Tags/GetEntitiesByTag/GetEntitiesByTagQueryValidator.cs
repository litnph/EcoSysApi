using FluentValidation;

namespace PFP.Application.Features.Tags.GetEntitiesByTag;

public sealed class GetEntitiesByTagQueryValidator : AbstractValidator<GetEntitiesByTagQuery>
{
    public GetEntitiesByTagQueryValidator() => RuleFor(x => x.TagId).NotEmpty();
}
