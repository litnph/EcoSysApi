using MediatR;

namespace PFP.Application.Features.Automation.GetAutomationRuleDetail;

public sealed record GetAutomationRuleDetailQuery(Guid Id) : IRequest<GetAutomationRuleDetailResponse>;
