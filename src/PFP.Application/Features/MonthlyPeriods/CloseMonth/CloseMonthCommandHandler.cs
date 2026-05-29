using System.Globalization;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.CloseMonth;

/// <summary>Validates billing cycles, freezes totals, and marks the month closed.</summary>
public sealed class CloseMonthCommandHandler : IRequestHandler<CloseMonthCommand, CloseMonthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public CloseMonthCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc cref="IRequestHandler{CloseMonthCommand, CloseMonthResponse}.Handle" />
    public async Task<CloseMonthResponse> Handle(CloseMonthCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var blockingCycles = await _db.FinBillingCycles
            .AsNoTracking()
            .Include(bc => bc.Source)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (blockingCycles.Count > 0)
        {
            var cardNames = string.Join(", ", blockingCycles.Select(bc => bc.Source.Name).Distinct());
            throw new BusinessRuleException(
                string.Format(CultureInfo.InvariantCulture, "Còn {0} kỳ sao kê chưa đóng: {1}", blockingCycles.Count, cardNames));
        }

        var report = await MonthlyPeriodSummaryCalculator
            .BuildReportAsync(_db, request.Year, request.Month, cancellationToken)
            .ConfigureAwait(false);

        var income = CurrencyUnits.FromWhole(report.Summary.TotalIncome);
        var expense = CurrencyUnits.FromWhole(report.Summary.TotalExpense);
        var net = CurrencyUnits.FromWhole(report.Summary.Net);
        var categories = report.CategoryBreakdown;
        var sources = report.SourceBreakdown;

        var categoryJson = MonthlyPeriodSummaryCalculator.SerialiseCategoryBreakdown(categories);
        var sourceJson = MonthlyPeriodSummaryCalculator.SerialiseSourceBreakdown(sources);

        var topCategories = categories
            .Where(c => c.CategoryId.HasValue)
            .Take(5)
            .Select(c => new CategoryAmountBreakdownDto(c.CategoryId!.Value, c.CategoryName, c.Amount))
            .ToList();

        var utcNow = DateTime.UtcNow;
        var userId = _currentUser.UserId.Value;

        // Compose the explicit transaction with the EF Core retrying execution strategy so transient
        // failures retry the whole close-month unit (spec §4.4).
        var strategy = _db.Database.CreateExecutionStrategy();
        var periodId = await strategy.ExecuteAsync(async () =>
        {
            await using var dbTx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var period = await _db.FinMonthlyPeriods
                .FirstOrDefaultAsync(
                    p => p.Year == request.Year && p.Month == request.Month,
                    cancellationToken)
                .ConfigureAwait(false);

            if (period is null)
            {
                period = new FinMonthlyPeriod
                {                    Year = request.Year,
                    Month = request.Month,
                };
                _db.FinMonthlyPeriods.Add(period);
            }

            if (period.Status == PeriodStatus.Closed)
                throw new BusinessRuleException("This month is already closed.");

            if (period.ReportCreatedAt is null)
                throw new BusinessRuleException("Create a monthly report before closing it.");

            period.TotalIncome = income;
            period.TotalExpense = expense;
            period.Net = net;
            period.CategoryBreakdown = categoryJson;
            period.SourceBreakdown = sourceJson;
            period.ReportSnapshot = MonthlyReportSnapshotStore.Serialize(report);
            period.LastRefreshedAt = utcNow;
            period.Status = PeriodStatus.Closed;
            period.ClosedAt = utcNow;
            period.ClosedBy = userId;

            var monthStart = new DateOnly(request.Year, request.Month, 1);
            var monthEnd = new DateOnly(request.Year, request.Month, DateTime.DaysInMonth(request.Year, request.Month));
            var monthTxns = await _db.FinTransactions
                .Where(t => t.TxnDate >= monthStart
                    && t.TxnDate <= monthEnd
                    && t.Status != TxnStatus.Cancelled
                    && t.Status != TxnStatus.Completed)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var txn in monthTxns)
            {
                txn.Status = TxnStatus.Completed;
                txn.MonthlyPeriodId = period.Id;
            }

            var closeAuditPayload = JsonSerializer.Serialize(new
            {                request.Year,
                request.Month,
                totalIncome = income,
                totalExpense = expense,
                net,
            });

            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                SessionId = _currentUser.SessionId,
                EntityType = "fin_monthly_period_close",
                EntityId = period.Id,
                Action = AuditAction.Created,
                BeforeSnapshot = null,
                AfterSnapshot = closeAuditPayload,
                ChangedFields = null,
                IpAddress = _currentUser.IpAddress,
                UserAgent = _currentUser.UserAgent,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
            });

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await dbTx.CommitAsync(cancellationToken).ConfigureAwait(false);

            return period.Id;
        }).ConfigureAwait(false);

        var dto = new MonthlyPeriodSummaryDto(
            periodId,
            request.Year,
            request.Month,
            PeriodStatus.Closed,
            utcNow,
            userId,
            CurrencyUnits.ToWhole(income),
            CurrencyUnits.ToWhole(expense),
            CurrencyUnits.ToWhole(net),
            topCategories,
            categories,
            sources);

        return new CloseMonthResponse(dto);
    }
}
