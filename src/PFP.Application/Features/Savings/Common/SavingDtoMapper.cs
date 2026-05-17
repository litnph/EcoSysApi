using PFP.Domain.Entities.Finance;

namespace PFP.Application.Features.Savings.Common;

internal static class SavingDtoMapper
{
    public static SavingListItemDto ToListItem(FinSaving s, string sourceName) =>
        new(
            s.Id,
            s.SmoduleId,
            s.SourceId,
            sourceName,
            s.Name,
            s.TargetAmount,
            s.CurrentAmount,
            s.InterestRate,
            s.StartDate,
            s.MaturityDate,
            s.Type,
            s.Status,
            s.Note);

    public static SavingDetailDto ToDetail(FinSaving s, string sourceName) =>
        new(
            s.Id,
            s.SmoduleId,
            s.SourceId,
            sourceName,
            s.Name,
            s.TargetAmount,
            s.CurrentAmount,
            s.InterestRate,
            s.StartDate,
            s.MaturityDate,
            s.Type,
            s.Status,
            s.Note,
            s.CreatedAt,
            s.UpdatedAt);
}
