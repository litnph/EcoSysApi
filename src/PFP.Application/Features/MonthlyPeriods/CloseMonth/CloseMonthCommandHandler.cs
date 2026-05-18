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

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to close months for this finance module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var blockingCycles = await _db.FinBillingCycles
            .AsNoTracking()
            .Include(bc => bc.Source)
            .Where(bc => bc.SmoduleId == request.SmoduleId
                         && bc.PeriodEnd.Year == request.Year
                         && bc.PeriodEnd.Month == request.Month
                         && bc.Status != BillingCycleStatus.Closed
                         && bc.Status != BillingCycleStatus.Paid)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (blockingCycles.Count > 0)
        {
            var cardNames = string.Join(", ", blockingCycles.Select(bc => bc.Source.Name).Distinct());
            throw new BusinessRuleException(
                string.Format(CultureInfo.InvariantCulture, "Còn {0} kỳ sao kê chưa đóng: {1}", blockingCycles.Count, cardNames));
        }

        var (income, expense, net, categories, sources) = await MonthlyPeriodSummaryCalculator
            .ComputeBreakdownsAsync(_db, request.SmoduleId, request.Year, request.Month, cancellationToken)
            .ConfigureAwait(false);

        var categoryJson = MonthlyPeriodSummaryCalculator.SerialiseCategoryBreakdown(categories);
        var sourceJson = MonthlyPeriodSummaryCalculator.SerialiseSourceBreakdown(sources);

        var topCategories = categories
            .Where(c => c.CategoryId.HasValue)
            .Take(5)
            .Select(c => new CategoryAmountBreakdownDto(c.CategoryId!.Value, c.CategoryName, c.Amount))
            .ToList();

        await using var dbTx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var period = await _db.FinMonthlyPeriods
            .FirstOrDefaultAsync(
                p => p.SmoduleId == request.SmoduleId && p.Year == request.Year && p.Month == request.Month,
                cancellationToken)
            .ConfigureAwait(false);

        if (period is null)
        {
            period = new FinMonthlyPeriod
            {
                SmoduleId = request.SmoduleId,
                Year = request.Year,
                Month = request.Month,
            };
            _db.FinMonthlyPeriods.Add(period);
        }

        if (period.Status == PeriodStatus.Closed)
            throw new BusinessRuleException("This month is already closed.");

        var utcNow = DateTime.UtcNow;
        var userId = _currentUser.UserId.Value;

        period.TotalIncome = income;
        period.TotalExpense = expense;
        period.Net = net;
        period.CategoryBreakdown = categoryJson;
        period.SourceBreakdown = sourceJson;
        period.Status = PeriodStatus.Closed;
        period.ClosedAt = utcNow;
        period.ClosedBy = userId;

        var closeAuditPayload = JsonSerializer.Serialize(new
        {
            smoduleId = request.SmoduleId,
            request.Year,
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

        var dto = new MonthlyPeriodSummaryDto(
            period.Id,
            request.SmoduleId,
            request.Year,
            request.Month,
            period.Status,
            period.ClosedAt,
            period.ClosedBy,
            CurrencyUnits.ToWhole(income),
            CurrencyUnits.ToWhole(expense),
            CurrencyUnits.ToWhole(net),
            topCategories,
            categories,
            sources);

        return new CloseMonthResponse(dto);
    }
}
