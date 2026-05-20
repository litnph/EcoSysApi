using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.FileAttachments.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.FileAttachments.ListTransactionAttachments;

public sealed class GetTransactionAttachmentsQueryHandler : IRequestHandler<GetTransactionAttachmentsQuery, GetTransactionAttachmentsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionAttachmentsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetTransactionAttachmentsResponse> Handle(
        GetTransactionAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        await FileAttachmentEntityAccess.RequireAttachmentTargetAsync(
                _db,
                _currentUser,
                nameof(FinTransaction),
                request.TransactionId,
                cancellationToken)
            .ConfigureAwait(false);

        var rows = await _db.FileAttachments.AsNoTracking()
            .Where(a => a.EntityType == nameof(FinTransaction) && a.EntityId == request.TransactionId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new FileAttachmentSummaryDto(
                a.Id,
                a.FileName,
                a.MimeType,
                a.FileSize,
                a.IsPublic,
                a.CreatedAt,
                a.UploadedBy))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetTransactionAttachmentsResponse(rows);
    }
}
