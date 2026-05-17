using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Options;
using PFP.Application.Features.FileAttachments.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.FileAttachments.GetFileUrl;

public sealed class GetFileUrlQueryHandler : IRequestHandler<GetFileUrlQuery, SignedFileUrlDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storage;
    private readonly FileStorageOptions _files;

    public GetFileUrlQueryHandler(
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

    public async Task<SignedFileUrlDto> Handle(GetFileUrlQuery request, CancellationToken cancellationToken)
    {
        var att = await _db.FileAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, cancellationToken)
            .ConfigureAwait(false);

        if (att is null)
            throw new NotFoundException("File attachment was not found.");

        await FileAttachmentEntityAccess.RequireAttachmentTargetAsync(
                _db,
                _currentUser,
                att.EntityType,
                att.EntityId,
                SpaceRole.Viewer,
                cancellationToken)
            .ConfigureAwait(false);

        var ttl = TimeSpan.FromMinutes(Math.Max(1, _files.DefaultSignedUrlMinutes));
        var expires = DateTime.UtcNow.Add(ttl);
        var url = await _storage.GetSignedUrlAsync(att.FileKey, ttl, cancellationToken).ConfigureAwait(false);

        return new SignedFileUrlDto(url, expires);
    }
}
