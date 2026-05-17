using MediatR;

namespace PFP.Application.Features.Automation.GetAutomationRules;

public sealed record GetAutomationRulesQuery(Guid SmoduleId) : IRequest<GetAutomationRulesResponse>;
