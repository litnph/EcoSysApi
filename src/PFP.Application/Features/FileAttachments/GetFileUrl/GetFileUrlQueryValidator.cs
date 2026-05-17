using FluentValidation;

namespace PFP.Application.Features.FileAttachments.GetFileUrl;

public sealed class GetFileUrlQueryValidator : AbstractValidator<GetFileUrlQuery>
{
    public GetFileUrlQueryValidator() => RuleFor(x => x.AttachmentId).NotEmpty();
}
