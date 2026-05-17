using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.InstallmentPlans.Common;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.GetInstallmentPlans;

/// <summary>Handles <see cref="GetInstallmentPlansQuery"/>.</summary>
public sealed class GetInstallmentPlansQueryHandler : IRequestHandler<GetInstallmentPlansQuery, GetInstallmentPlansResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetInstallmentPlansQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetInstallmentPlansResponse> Handle(GetInstallmentPlansQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to view installment plans for this module.");

        var q = _db.FinInstallmentPlans
            .AsNoTracking()
            .Include(p => p.Source)
            .Include(p => p.OriginalTransaction)
            .Include(p => p.Pays)
            .Where(p => p.SmoduleId == request.SmoduleId);

        if (request.Status is { } st)
            q = q.Where(p => p.Status == st);

        var rows = await q
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = rows.Select(Map).ToList();
        return new GetInstallmentPlansResponse(items);
    }

    private static InstallmentPlanListItemDto Map(FinInstallmentPlan p)
    {
        var paid = p.Pays.Count(x => x.Status == InstallmentPayStatus.Paid);
        var remaining = p.Pays.Where(x => x.Status != InstallmentPayStatus.Paid).Sum(x => x.Amount);
        return new InstallmentPlanListItemDto(
            p.Id,
            p.SmoduleId,
            p.SourceId,
            p.Source.Name,
            p.OriginalTransaction.Description,
            p.Status,
            paid,
            p.TotalMonths,
            remaining,
            p.CreatedAt);
    }
}
