using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.FileAttachments.Common;
using PFP.Application.Features.FileAttachments.DeleteFile;
using PFP.Application.Features.FileAttachments.GetFileUrl;
using PFP.Application.Features.FileAttachments.UploadFile;

namespace PFP.API.Controllers;

/// <summary>Multipart uploads, presigned URLs, and soft-removal for file attachments.</summary>
[ApiController]
[Authorize]
[Route("api/v1/files")]
public sealed class FileAttachmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public FileAttachmentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Acknowledges multipart upload bounds for clients that temporarily exceed defaults.</summary>
    public const int MultipartMaxBytes = 52_428_800; // 50 MiB

    /// <summary>Stores payload in R2 and creates a linked <see cref="PFP.Domain.Entities.FileAttachment"/> row.</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = MultipartMaxBytes)]
    [RequestSizeLimit(MultipartMaxBytes)]
    [ProducesResponseType(typeof(ApiResponse<FileAttachmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FileAttachmentDto>>> Upload(
        [FromForm] UploadFileForm form,
        CancellationToken cancellationToken = default)
    {
        var maxFileSizeMb = form.MaxFileSizeMb <= 0 ? 10 : form.MaxFileSizeMb;

        if (form.File is null || form.File.Length <= 0)
            return BadRequest(new ApiResponse<FileAttachmentDto>
            {
                Success = false,
                Error = new { message = "A non-empty multipart file field is required." },
            });

        await using var content = form.File.OpenReadStream();
        var resolvedMimeType = string.IsNullOrWhiteSpace(form.MimeType)
            ? form.File.ContentType ?? ""
            : form.MimeType!;
        var command = new UploadFileCommand(
            form.ModuleCode.Trim(),
            form.EntityType.Trim(),
            form.EntityId,
            form.File.FileName,
            resolvedMimeType.Trim(),
            content,
            form.File.Length,
            maxFileSizeMb,
            form.IsPublic);

        var created = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<FileAttachmentDto> { Data = created });
    }

    /// <summary>Issues a fresh presigned HTTPS GET URL (previous links may expire).</summary>
    [HttpGet("{id:guid}/url")]
    [ProducesResponseType(typeof(ApiResponse<SignedFileUrlDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SignedFileUrlDto>>> GetSignedUrl(Guid id, CancellationToken cancellationToken)
    {
        var url = await _mediator.Send(new GetFileUrlQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<SignedFileUrlDto> { Data = url });
    }

    /// <summary>Soft-deletes the row and deletes the backing object.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteFileCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { id } });
    }
}
