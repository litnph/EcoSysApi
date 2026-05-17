using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Gdpr.ExportData;

/// <summary>Returns export metadata + signed download link when ready.</summary>
public sealed class GetDataExportByIdQueryHandler : IRequestHandler<GetDataExportByIdQuery, DataExportStatusDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    /// <summary>Creates the handler.</summary>
    public GetDataExportByIdQueryHandler(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    /// <inheritdoc/>
    public async Task<DataExportStatusDto> Handle(GetDataExportByIdQuery request, CancellationToken cancellationToken)
    {
        if (_current.UserId is not Guid userId)
            throw new UnauthorizedAppException("Authentication is required.");

        var row = await _db.UserDataExports.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.ExportId && e.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
            throw new NotFoundException("Data export was not found.");

        return new DataExportStatusDto(
            row.Id,
            row.Status,
            row.DownloadUrl,
            row.SizeBytes,
            row.ExpiresAt,
            row.ProcessedAt,
            row.ReadyAt,
            row.ErrorMessage);
    }
}
