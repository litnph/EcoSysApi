using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.InstallmentPlans.Common;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.GetInstallmentDashboard;

public sealed class GetInstallmentDashboardQueryHandler
    : IRequestHandler<GetInstallmentDashboardQuery, GetInstallmentDashboardResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetInstallmentDashboardQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetInstallmentDashboardResponse> Handle(
        GetInstallmentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thisMonthStart = new DateOnly(today.Year, today.Month, 1);
        var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);
        var nextMonthStart = thisMonthStart.AddMonths(1);
        var nextMonthEnd = nextMonthStart.AddMonths(1).AddDays(-1);

        var plans = await _db.FinInstallmentPlans
            .AsNoTracking()
            .Include(p => p.Source)
            .Include(p => p.Pays)
            .Include(p => p.OriginalTransaction)
                .ThenInclude(t => t.Category)
            .Where(p => p.Status == InstallmentStatus.Active)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        decimal totalRemaining = 0;
        decimal totalOriginal = 0;
        decimal totalPaid = 0;
        var dueCount = 0;
        decimal dueAmount = 0;
        var overdueCount = 0;
        decimal overdueAmount = 0;
        var upcomingCount = 0;
        decimal upcomingAmount = 0;
        var thisMonthDueCount = 0;
        decimal thisMonthDueAmount = 0;
        var nextMonthDueCount = 0;
        decimal nextMonthDueAmount = 0;

        var sourceMap = new Dictionary<Guid, SourceAccumulator>();
        var upcomingPays = new List<InstallmentUpcomingPayDto>();

        foreach (var plan in plans)
        {
            totalOriginal += plan.TotalAmount;

            var sourceStats = GetOrCreateSource(sourceMap, plan);

            foreach (var pay in plan.Pays)
            {
                totalPaid += pay.PaidAmount;

                if (pay.Status == InstallmentPayStatus.Paid)
                    continue;

                var open = pay.Amount - pay.PaidAmount;
                if (open <= 0)
                    continue;

                totalRemaining += open;

                var bucket = ResolveBucket(pay, today);
                switch (bucket)
                {
                    case InstallmentUpcomingPayBucket.Overdue:
                        overdueCount++;
                        overdueAmount += open;
                        sourceStats.OverdueAmount += open;
                        break;
                    case InstallmentUpcomingPayBucket.DueToday:
                        dueCount++;
                        dueAmount += open;
                        break;
                    case InstallmentUpcomingPayBucket.ThisMonth:
                        thisMonthDueCount++;
                        thisMonthDueAmount += open;
                        sourceStats.ThisMonthDueAmount += open;
                        break;
                    case InstallmentUpcomingPayBucket.NextMonth:
                        nextMonthDueCount++;
                        nextMonthDueAmount += open;
                        sourceStats.NextMonthDueAmount += open;
                        break;
                    case InstallmentUpcomingPayBucket.Later:
                        upcomingCount++;
                        upcomingAmount += open;
                        break;
                }

                sourceStats.RemainingAmount += open;

                if (bucket is InstallmentUpcomingPayBucket.Overdue
                    or InstallmentUpcomingPayBucket.DueToday
                    or InstallmentUpcomingPayBucket.ThisMonth
                    or InstallmentUpcomingPayBucket.NextMonth
                    or InstallmentUpcomingPayBucket.Later)
                {
                    upcomingPays.Add(new InstallmentUpcomingPayDto(
                        plan.Id,
                        plan.SourceId,
                        plan.Source.Name,
                        plan.Source.Icon,
                        InstallmentPlanRules.ResolveOriginalTxnTitle(plan),
                        pay.InstallmentNumber,
                        plan.TotalMonths,
                        pay.DueDate,
                        CurrencyUnits.ToWhole(open),
                        bucket));
                }
            }
        }

        var completionPercent = totalOriginal <= 0
            ? 100
            : (int)Math.Round(Math.Min(100, totalPaid / totalOriginal * 100), MidpointRounding.AwayFromZero);

        var bySource = sourceMap.Values
            .OrderByDescending(s => s.RemainingAmount)
            .ThenBy(s => s.SourceName, StringComparer.OrdinalIgnoreCase)
            .Select(s => new InstallmentDashboardSourceDto(
                s.SourceId,
                s.SourceName,
                s.SourceIcon,
                s.SourceColor,
                s.ActivePlanCount,
                CurrencyUnits.ToWhole(s.RemainingAmount),
                CurrencyUnits.ToWhole(s.OverdueAmount),
                CurrencyUnits.ToWhole(s.ThisMonthDueAmount),
                CurrencyUnits.ToWhole(s.NextMonthDueAmount)))
            .ToList();

        var orderedPays = upcomingPays
            .OrderBy(p => p.Bucket)
            .ThenBy(p => p.DueDate)
            .ThenBy(p => p.SourceName, StringComparer.OrdinalIgnoreCase)
            .Take(30)
            .ToList();

        var dashboard = new InstallmentDashboardDto(
            plans.Count,
            CurrencyUnits.ToWhole(totalRemaining),
            dueCount,
            CurrencyUnits.ToWhole(dueAmount),
            overdueCount,
            CurrencyUnits.ToWhole(overdueAmount),
            upcomingCount,
            CurrencyUnits.ToWhole(upcomingAmount),
            thisMonthDueCount,
            CurrencyUnits.ToWhole(thisMonthDueAmount),
            nextMonthDueCount,
            CurrencyUnits.ToWhole(nextMonthDueAmount),
            completionPercent,
            bySource,
            orderedPays);

        return new GetInstallmentDashboardResponse(dashboard);
    }

    private static InstallmentUpcomingPayBucket ResolveBucket(
        FinInstallmentPay pay,
        DateOnly today)
    {
        if (pay.DueDate < today)
            return InstallmentUpcomingPayBucket.Overdue;

        if (pay.DueDate == today || pay.Status == InstallmentPayStatus.Due)
            return InstallmentUpcomingPayBucket.DueToday;

        var thisMonthStart = new DateOnly(today.Year, today.Month, 1);
        var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);
        var nextMonthStart = thisMonthStart.AddMonths(1);
        var nextMonthEnd = nextMonthStart.AddMonths(1).AddDays(-1);

        if (pay.DueDate >= thisMonthStart && pay.DueDate <= thisMonthEnd)
            return InstallmentUpcomingPayBucket.ThisMonth;

        if (pay.DueDate >= nextMonthStart && pay.DueDate <= nextMonthEnd)
            return InstallmentUpcomingPayBucket.NextMonth;

        return InstallmentUpcomingPayBucket.Later;
    }

    private static SourceAccumulator GetOrCreateSource(
        Dictionary<Guid, SourceAccumulator> map,
        FinInstallmentPlan plan)
    {
        if (map.TryGetValue(plan.SourceId, out var existing))
        {
            existing.ActivePlanCount++;
            return existing;
        }

        var created = new SourceAccumulator(
            plan.SourceId,
            plan.Source.Name,
            plan.Source.Icon,
            plan.Source.Color);
        map[plan.SourceId] = created;
        return created;
    }

    private sealed class SourceAccumulator
    {
        public SourceAccumulator(Guid sourceId, string sourceName, string? sourceIcon, string? sourceColor)
        {
            SourceId = sourceId;
            SourceName = sourceName;
            SourceIcon = sourceIcon;
            SourceColor = sourceColor;
            ActivePlanCount = 1;
        }

        public Guid SourceId { get; }
        public string SourceName { get; }
        public string? SourceIcon { get; }
        public string? SourceColor { get; }
        public int ActivePlanCount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal ThisMonthDueAmount { get; set; }
        public decimal NextMonthDueAmount { get; set; }
    }
}
