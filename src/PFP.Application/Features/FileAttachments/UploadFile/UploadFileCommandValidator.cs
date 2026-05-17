using FluentValidation;
using PFP.Application.Features.FileAttachments.Common;

namespace PFP.Application.Features.FileAttachments.UploadFile;

public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileContent).NotNull();

        RuleFor(x => x.DeclaredContentLength)
            .GreaterThan(0)
            .LessThanOrEqualTo(cmd => cmd.MaxFileSizeMb * 1024L * 1024L)
            .WithMessage(cmd => $"File must be smaller than {cmd.MaxFileSizeMb} MB.");

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .Must(m => FileAttachmentAllowedMimeTypes.All.Contains(m))
            .WithMessage("This file type is not allowed.");

        RuleFor(x => x.ModuleCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.MaxFileSizeMb).GreaterThan(0).LessThanOrEqualTo(250);
    }
}
