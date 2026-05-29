using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PFP.Application.Common;
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
        if (_currentUser.IsAuthenticated && _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new NotFoundException("Source was not found.");

        if (source.Type != SourceType.CreditCard)
            throw new BusinessRuleException("Only credit-card sources can have billing cycles.");

        if (source.StatementDay is not { } statementDay || source.PaymentDueDay is not { } paymentDueDay)
            throw new BusinessRuleException("StatementDay and PaymentDueDay are required on the credit card.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        BillingCyclePeriodDates dates;

        if (request.StatementYear is { } year && request.StatementMonth is { } month)
        {
            dates = BillingCycleCalendar.ResolveForStatementMonth(
                year, month, statementDay, paymentDueDay);
        }
        else if (request.StatementYear is null && request.StatementMonth is null)
        {
            dates = BillingCycleCalendar.ResolveCurrentPeriod(today, statementDay, paymentDueDay);
        }
        else
        {
            throw new BusinessRuleException("Statement year and month must both be provided or both omitted.");
        }

        var duplicate = await _db.FinBillingCycles
            .AsNoTracking()
            .AnyAsync(
                bc => bc.SourceId == source.Id && bc.PeriodStart == dates.PeriodStart,
                cancellationToken)
            .ConfigureAwait(false);

        if (duplicate)
            throw new BusinessRuleException("A billing cycle for this statement period already exists.");

        var cycle = new FinBillingCycle
        {
            SourceId = source.Id,
            Name = BillingCycleNaming.BuildDefaultName(dates.StatementDate),
            PeriodStart = dates.PeriodStart,
            PeriodEnd = dates.PeriodEnd,
            StatementDate = dates.StatementDate,
            PaymentDueDate = dates.PaymentDueDate,
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

        var trackedCycle = await _db.FinBillingCycles
            .FirstAsync(c => c.Id == cycle.Id, cancellationToken)
            .ConfigureAwait(false);
        await BillingCycleTotals.RecalculateAsync(trackedCycle, _db, cancellationToken)
            .ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new GenerateBillingCycleResponse(FinBillingCycleDtoMapper.ToDto(trackedCycle, source.Name));
    }
}
