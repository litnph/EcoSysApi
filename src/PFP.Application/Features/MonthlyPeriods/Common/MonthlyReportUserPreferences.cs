using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

internal static class MonthlyReportUserPreferences
{
    internal readonly record struct Values(
        int ReportDay,
        MonthlyReportPeriodMode PeriodMode);

    public static async Task<Values> GetAsync(
        IApplicationDbContext db,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var saved = await db.UserProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => new
            {
                profile.MonthlyReportDay,
                profile.MonthlyReportPeriodMode,
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return saved is null
            ? new Values(1, MonthlyReportPeriodMode.LowerBoundary)
            : new Values(saved.MonthlyReportDay, saved.MonthlyReportPeriodMode);
    }
}
