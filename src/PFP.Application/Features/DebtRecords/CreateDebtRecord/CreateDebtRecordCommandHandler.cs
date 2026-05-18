using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.DebtRecords.GetDebtRecordDetail;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.CreateDebtRecord;

/// <summary>
/// Inserts a manually-created debt record. Does NOT touch <c>FIN_SOURCES.balance</c> or create a
/// <c>FIN_TRANSACTION</c> — the cash movement happened outside the platform and the user is just
/// catching the books up.
/// </summary>
public sealed class CreateDebtRecordCommandHandler : IRequestHandler<CreateDebtRecordCommand, CreateDebtRecordResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public CreateDebtRecordCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc cref="IRequestHandler{CreateDebtRecordCommand, CreateDebtRecordResponse}.Handle" />
    public async Task<CreateDebtRecordResponse> Handle(CreateDebtRecordCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new ForbiddenException("You do not have permission to manage debt records for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new ForbiddenException("The current organisation does not own this space module.");

        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? "VND"
            : request.Currency.Trim().ToUpperInvariant();

        var amount = CurrencyUnits.FromWhole(request.Amount);
        var debt = new FinDebtRecord
        {
            SmoduleId = request.SmoduleId,
            Direction = request.Direction,
            PersonName = request.PersonName.Trim(),
            PersonContact = string.IsNullOrWhiteSpace(request.PersonContact) ? null : request.PersonContact.Trim(),
            OriginalTxnId = null,
            OriginalAmount = amount,
            RemainingAmount = amount,
            Currency = currency,
            DueDate = request.DueDate,
            Status = DebtStatus.Active,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.FinDebtRecords.Add(debt);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        var dto = new DebtRecordDetailDto(
            debt.Id,
            debt.SmoduleId,
            debt.Direction,
            debt.PersonName,
            debt.PersonContact,
            debt.OriginalTxnId,
            CurrencyUnits.ToWhole(debt.OriginalAmount),
            CurrencyUnits.ToWhole(debt.RemainingAmount),
            debt.Currency,
            debt.DueDate,
            debt.Status,
            debt.Note,
            debt.Version,
            debt.CreatedAt,
            debt.UpdatedAt,
            Array.Empty<DebtTransactionItemDto>());

        return new CreateDebtRecordResponse(dto);
    }
}
