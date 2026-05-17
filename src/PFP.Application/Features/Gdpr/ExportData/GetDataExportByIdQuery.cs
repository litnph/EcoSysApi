using MediatR;

namespace PFP.Application.Features.Gdpr.ExportData;

/// <summary>Loads export status for the authenticated owner.</summary>
public sealed record GetDataExportByIdQuery(Guid ExportId) : IRequest<DataExportStatusDto>;
