using FluentValidation;

namespace PFP.Application.Features.Tags.GetTags;

public sealed class GetTagsQueryValidator : AbstractValidator<GetTagsQuery>
{
    public GetTagsQueryValidator() => RuleFor(x => x.SmoduleId).NotEmpty();
}
