using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.ImageImportSettings;

public sealed class ImageImportTypeSettingsHandlers :
    IRequestHandler<GetImageImportTypeSettingsQuery, IReadOnlyList<ImageImportTypeSettingDto>>,
    IRequestHandler<UpdateImageImportTypeSettingsCommand, IReadOnlyList<ImageImportTypeSettingDto>>
{
    private readonly IApplicationDbContext _db;

    public ImageImportTypeSettingsHandlers(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ImageImportTypeSettingDto>> Handle(
        GetImageImportTypeSettingsQuery request,
        CancellationToken cancellationToken) =>
        await LoadAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<ImageImportTypeSettingDto>> Handle(
        UpdateImageImportTypeSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var settings = ImageImportTypeSettingsRules.Normalize(request.Settings);
        var sourceIds = settings
            .Where(setting => setting.SourceId.HasValue)
            .Select(setting => setting.SourceId!.Value)
            .Distinct()
            .ToArray();

        if (sourceIds.Length > 0)
        {
            var sourceCount = await _db.FinSources.AsNoTracking()
                .CountAsync(source => sourceIds.Contains(source.Id), cancellationToken)
                .ConfigureAwait(false);
            if (sourceCount != sourceIds.Length)
                throw new BusinessRuleException("One or more image-import money sources are not available.");
        }

        var existing = await _db.ImageImportTypeConfigurations
            .ToDictionaryAsync(setting => setting.Type, StringComparer.Ordinal, cancellationToken)
            .ConfigureAwait(false);

        foreach (var setting in settings)
        {
            if (!existing.TryGetValue(setting.Type, out var entity))
            {
                entity = new ImageImportTypeConfiguration { Type = setting.Type };
                _db.ImageImportTypeConfigurations.Add(entity);
            }

            entity.DisplayName = setting.DisplayName;
            entity.SourceId = setting.SourceId;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await LoadAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<ImageImportTypeSettingDto>> LoadAsync(
        CancellationToken cancellationToken) =>
        await _db.ImageImportTypeConfigurations.AsNoTracking()
            .OrderBy(setting => setting.Type)
            .Select(setting => new ImageImportTypeSettingDto(
                setting.Type,
                setting.DisplayName,
                setting.SourceId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
