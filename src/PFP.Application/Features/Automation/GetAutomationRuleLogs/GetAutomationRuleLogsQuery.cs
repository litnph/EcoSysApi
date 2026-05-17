using MediatR;

namespace PFP.Application.Features.Automation.GetAutomationRuleLogs;

public sealed record GetAutomationRuleLogsQuery(Guid RuleId, int Page = 1, int PageSize = 20)
    : IRequest<GetAutomationRuleLogsResponse>;
