using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Application.Features.InstallmentPlans.Commands.ProcessConversionFee;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;

/// <summary>Persists a new <see cref="FinBillingCycle"/> row for the requested credit card.</summary>
public sealed class GenerateBillingCycleCommandHandler : IRequestHandler<GenerateBillingCycleCommand, GenerateBillingCycleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateBillingCycleCommandHandler> _logger;

    /// <summary>Creates the handler.</summary>
    public GenerateBillingCycleCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IMediator mediator,
        ILogger<GenerateBillingCycleCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<GenerateBillingCycleResponse> Handle(GenerateBillingCycleCommand request, CancellationToken cancellationToken)
    {
        var source = await _db.FinSources
            .Include(s => s.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new NotFoundException("Source was not found.");

        if (_currentUser.IsAuthenticated)
        {
            if (_currentUser.UserId is null)
                throw new UnauthorizedAppException("Authentication is required.");

            if (!await _currentUser
                    .HasSpaceModuleAccessAsync(source.SmoduleId, SpaceRole.Editor, cancellationToken)
                    .ConfigureAwait(false))
                throw new UnauthorizedAppException("You do not have permission to generate billing cycles for this module.");

            if (_currentUser.CurrentOrgId is { } orgId && source.Smodule.Space.OrgId != orgId)
                throw new UnauthorizedAppException("The current organisation does not own this source.");
        }
        else
        {
            // Hangfire / system jobs — no end-user principal; trust caller (scheduled job) after source exists.
            if (!source.Smodule.IsEnabled || source.Smodule.ModuleCode != ModuleCode.Finance)
                throw new UnauthorizedAppException("The finance module is not active for this source.");
        }

        if (source.Type != SourceType.CreditCard)
            throw new BusinessRuleException("Only credit-card sources can have billing cycles.");

        if (source.StatementDay is not { } statementDay || source.PaymentDueDay is not { } paymentDueDay)
            throw new BusinessRuleException("StatementDay and PaymentDueDay are required on the credit card.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var prev = today.AddMonths(-1);
        var periodStart = BillingCycleCalendar.DayInMonth(prev.Year, prev.Month, statementDay);
        var statementDate = BillingCycleCalendar.DayInMonth(today.Year, today.Month, statementDay);
        var periodEnd = statementDate.AddDays(-1);
        var paymentDueDate = statementDate.AddDays(paymentDueDay);

        var duplicate = await _db.FinBillingCycles
            .AsNoTracking()
            .AnyAsync(bc => bc.SourceId == source.Id && bc.PeriodStart == periodStart, cancellationToken)
            .ConfigureAwait(false);

        if (duplicate)
            throw new BusinessRuleException("A billing cycle for this statement period already exists.");

        var cycle = new FinBillingCycle
        {
            SmoduleId = source.SmoduleId,
            SourceId = source.Id,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            StatementDate = statementDate,
            PaymentDueDate = paymentDueDate,
            TotalAmount = 0,
            PaidAmount = 0,
            Status = BillingCycleStatus.Open,
        };

        _db.FinBillingCycles.Add(cycle);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _mediator
                .Send(new ProcessConversionFeeCommand(cycle.Id), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessConversionFee failed after creating billing cycle {CycleId}", cycle.Id);
        }

        return new GenerateBillingCycleResponse(FinBillingCycleDtoMapper.ToDto(cycle, source.Name));
    }
}
