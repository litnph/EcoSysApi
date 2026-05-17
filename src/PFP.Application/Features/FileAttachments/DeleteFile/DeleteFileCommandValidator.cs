using FluentValidation;

namespace PFP.Application.Features.FileAttachments.DeleteFile;

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
