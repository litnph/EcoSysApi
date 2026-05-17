using MediatR;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Gdpr.ExportData;

/// <summary>Creates export row, schedules Hangfire worker.</summary>
public sealed class RequestDataExportCommandHandler : IRequestHandler<RequestDataExportCommand, RequestDataExportResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IDataExportJobScheduler _scheduler;

    /// <summary>Creates the handler.</summary>
    public RequestDataExportCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService current,
        IDataExportJobScheduler scheduler)
    {
        _db = db;
        _current = current;
        _scheduler = scheduler;
    }

    /// <inheritdoc/>
    public async Task<RequestDataExportResponse> Handle(RequestDataExportCommand request, CancellationToken cancellationToken)
    {
        if (_current.UserId is not Guid userId)
            throw new UnauthorizedAppException("Authentication is required.");

        var export = new UserDataExport
        {
            UserId = userId,
            Status = DataExportStatus.Pending,
        };

        _db.UserDataExports.Add(export);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _scheduler.EnqueueProcessExport(export.Id);

        return new RequestDataExportResponse(export.Id);
    }
}
