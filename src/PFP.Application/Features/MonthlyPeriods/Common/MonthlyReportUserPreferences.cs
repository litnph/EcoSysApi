using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.MonthlyPeriods.Common;

internal static class MonthlyReportUserPreferences
{
    public static async Task<int> GetReportDayAsync(
        IApplicationDbContext db,
        Guid userId,
        string? existingReportSnapshot,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(existingReportSnapshot))
        {
            var savedDay = MonthlyReportSnapshotStore
                .Deserialize(existingReportSnapshot)
                .Metadata?
                .MonthlyReportDay;
            if (savedDay is >= 1 and <= 31)
                return savedDay.Value;
        }

        return await GetReportDayAsync(db, userId, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<int> GetReportDayAsync(
        IApplicationDbContext db,
        Guid userId,
        CancellationToken cancellationToken) =>
        await db.UserProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => (int?)profile.MonthlyReportDay)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? 1;
}
