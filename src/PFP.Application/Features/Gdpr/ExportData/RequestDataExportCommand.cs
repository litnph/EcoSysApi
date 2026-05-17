using MediatR;

namespace PFP.Application.Features.Gdpr.ExportData;

/// <summary>Queues a GDPR JSON export for the current user.</summary>
public sealed record RequestDataExportCommand : IRequest<RequestDataExportResponse>;

/// <summary>Identifier of the <see cref="Domain.Entities.UserDataExport"/> row.</summary>
public sealed record RequestDataExportResponse(Guid ExportId);
