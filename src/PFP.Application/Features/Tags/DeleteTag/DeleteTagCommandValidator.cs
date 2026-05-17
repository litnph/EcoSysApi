using FluentValidation;

namespace PFP.Application.Features.Tags.DeleteTag;

public sealed class DeleteTagCommandValidator : AbstractValidator<DeleteTagCommand>
{
    public DeleteTagCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
