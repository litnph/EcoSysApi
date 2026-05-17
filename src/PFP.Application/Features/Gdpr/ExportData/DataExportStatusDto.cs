using PFP.Domain.Enums;

namespace PFP.Application.Features.Gdpr.ExportData;

/// <summary>GET payload for a single export.</summary>
public sealed record DataExportStatusDto(
    Guid Id,
    DataExportStatus Status,
    string? DownloadUrl,
    long? SizeBytes,
    DateTime? ExpiresAt,
    DateTime? ProcessedAt,
    DateTime? ReadyAt,
    string? ErrorMessage);
