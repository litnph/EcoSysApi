using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Automation.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.GetAutomationRules;

public sealed class GetAutomationRulesQueryHandler : IRequestHandler<GetAutomationRulesQuery, GetAutomationRulesResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAutomationRulesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetAutomationRulesResponse> Handle(GetAutomationRulesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var rows = await _db.AutomationRules
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = rows.ConvertAll(AutomationRuleDtoMapper.ToListItem);
        return new GetAutomationRulesResponse(dtos);
    }
}
