using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Automation.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.GetAutomationRuleLogs;

public sealed class GetAutomationRuleLogsQueryHandler : IRequestHandler<GetAutomationRuleLogsQuery, GetAutomationRuleLogsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAutomationRuleLogsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetAutomationRuleLogsResponse> Handle(GetAutomationRuleLogsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var rule = await _db.AutomationRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RuleId, cancellationToken)
            .ConfigureAwait(false);

        if (rule is null)
            throw new NotFoundException("Automation rule was not found.");
var total = await _db.AutomationLogs
            .AsNoTracking()
            .Where(l => l.RuleId == request.RuleId)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        var rows = await _db.AutomationLogs
            .AsNoTracking()
            .Where(l => l.RuleId == request.RuleId)
            .OrderByDescending(l => l.TriggeredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = rows.ConvertAll(l => new AutomationLogEntryDto(
            l.Id,
            l.TriggeredAt,
            l.Status,
            l.ActionsExecuted,
            l.ErrorMessage,
            l.DurationMs));

        return new GetAutomationRuleLogsResponse(items, request.Page, request.PageSize, total, totalPages);
    }
}
