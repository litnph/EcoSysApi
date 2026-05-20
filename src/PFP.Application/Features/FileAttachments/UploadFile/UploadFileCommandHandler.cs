using MediatR;
using Microsoft.Extensions.Options;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Options;
using PFP.Application.Features.FileAttachments.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.FileAttachments.UploadFile;

public sealed class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, FileAttachmentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storage;
    private readonly FileStorageOptions _files;

    public UploadFileCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IStorageService storage,
        IOptions<FileStorageOptions> files)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _files = files.Value;
    }

    public async Task<FileAttachmentDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var moduleNorm = NormalizeModuleCode(request.ModuleCode);

        await FileAttachmentEntityAccess.RequireAttachmentTargetAsync(
                _db,
                _currentUser,
                request.EntityType,
                request.EntityId,
                cancellationToken)
            .ConfigureAwait(false);

        if (request.FileContent.CanSeek && request.FileContent.Length != request.DeclaredContentLength)
            throw new BusinessRuleException("Reported file size does not match the uploaded stream.");

        if (request.FileContent.CanSeek)
            request.FileContent.Position = 0;

        var ext = FileAttachmentAllowedMimeTypes.ToFileExtension(request.MimeType);
        var blobId = Guid.NewGuid().ToString("D");
        var key = $"{moduleNorm}/{request.EntityType}/{request.EntityId}/{blobId}.{ext}";

        var safeName = FileAttachmentAllowedMimeTypes.SanitizeOriginalFileName(request.FileName);

        await _storage.UploadAsync(request.FileContent, key, request.MimeType, cancellationToken).ConfigureAwait(false);

        try
        {
            var entity = new FileAttachment
            {
                ModuleCode = moduleNorm,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                FileName = safeName,
                FileKey = key,
                MimeType = request.MimeType.ToLowerInvariant(),
                FileSize = request.DeclaredContentLength,
                UploadedBy = _currentUser.UserId.Value,
                IsPublic = request.IsPublic,
            };

            _db.FileAttachments.Add(entity);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var ttl = TimeSpan.FromMinutes(Math.Max(1, _files.DefaultSignedUrlMinutes));
            var url = await _storage.GetSignedUrlAsync(key, ttl, cancellationToken).ConfigureAwait(false);

            return new FileAttachmentDto(
                entity.Id,
                entity.ModuleCode,
                entity.EntityType,
                entity.EntityId,
                entity.FileName,
                entity.MimeType,
                entity.FileSize,
                entity.IsPublic,
                entity.CreatedAt,
                url);
        }
        catch
        {
            try
            {
                await _storage.DeleteAsync(key, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Best-effort compensating delete; original exception will still surface.
            }

            throw;
        }
    }

    private static string NormalizeModuleCode(string raw)
    {
        var trimmed = raw.Trim().ToLowerInvariant();
        return trimmed switch
        {
            "finance" => trimmed,
            _ => throw new BusinessRuleException($"Module '{raw}' does not accept file uploads."),
        };
    }
}
