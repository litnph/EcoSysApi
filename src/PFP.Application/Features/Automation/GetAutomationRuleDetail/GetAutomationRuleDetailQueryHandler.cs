using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Automation.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.GetAutomationRuleDetail;

public sealed class GetAutomationRuleDetailQueryHandler : IRequestHandler<GetAutomationRuleDetailQuery, GetAutomationRuleDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAutomationRuleDetailQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetAutomationRuleDetailResponse> Handle(GetAutomationRuleDetailQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.AutomationRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Automation rule was not found.");
return new GetAutomationRuleDetailResponse(AutomationRuleDtoMapper.ToDetail(entity));
    }
}
