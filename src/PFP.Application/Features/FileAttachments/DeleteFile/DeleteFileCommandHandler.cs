using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.FileAttachments.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.FileAttachments.DeleteFile;

public sealed class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storage;

    public DeleteFileCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IStorageService storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<Unit> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var att = await _db.FileAttachments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (att is null)
            throw new NotFoundException("File attachment was not found.");

        await FileAttachmentEntityAccess.RequireAttachmentTargetAsync(
                _db,
                _currentUser,
                att.EntityType,
                att.EntityId,
                cancellationToken)
            .ConfigureAwait(false);

        var storedKey = att.FileKey;

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            _db.FileAttachments.Remove(att);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            await _storage.DeleteAsync(storedKey, ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
