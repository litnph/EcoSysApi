using FluentValidation;
using Microsoft.Extensions.Options;
using PFP.Application.Common.Options;
using PFP.Application.Features.FileAttachments.Common;

namespace PFP.Application.Features.FileAttachments.UploadFile;

public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator(IOptions<FileStorageOptions> options)
    {
        var serverMaxFileSizeMb = Math.Clamp(options.Value.MaxUploadSizeMb, 1, 50);

        RuleFor(x => x.FileContent).NotNull();

        RuleFor(x => x.DeclaredContentLength)
            .GreaterThan(0)
            .LessThanOrEqualTo(cmd => Math.Min(cmd.MaxFileSizeMb, serverMaxFileSizeMb) * 1024L * 1024L)
            .WithMessage($"File must be no larger than the server limit of {serverMaxFileSizeMb} MB.");

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .Must(m => FileAttachmentAllowedMimeTypes.All.Contains(m))
            .WithMessage("This file type is not allowed.");

        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.MaxFileSizeMb)
            .GreaterThan(0)
            .LessThanOrEqualTo(serverMaxFileSizeMb)
            .WithMessage($"Client upload limit cannot exceed the server limit of {serverMaxFileSizeMb} MB.");
    }
}
