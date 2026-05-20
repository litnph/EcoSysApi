using MediatR;
using PFP.Application.Common;
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
    private readonly IMediator _mediator;

    /// <summary>Creates the handler.</summary>
    public CreateDebtRecordCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IMediator mediator)
    {
        _db = db;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    /// <inheritdoc cref="IRequestHandler{CreateDebtRecordCommand, CreateDebtRecordResponse}.Handle" />
    public async Task<CreateDebtRecordResponse> Handle(CreateDebtRecordCommand request, CancellationToken cancellationToken)
    {
        FinanceAccessHelper.RequireAuthenticated(_currentUser);

        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? "VND"
            : request.Currency.Trim().ToUpperInvariant();

        var amount = CurrencyUnits.FromWhole(request.Amount);
        var debt = new FinDebtRecord
        {
            Direction = request.Direction,
            PersonName = request.PersonName.Trim(),
            PersonContact = string.IsNullOrWhiteSpace(request.PersonContact) ? null : request.PersonContact.Trim(),
            OriginalTxnId = null,
            OriginalAmount = amount,
            RemainingAmount = amount,
            Currency = currency,
            Status = DebtStatus.Active,
            DueDate = request.DueDate,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.FinDebtRecords.Add(debt);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        var detail = await _mediator.Send(new GetDebtRecordDetailQuery(debt.Id), cancellationToken).ConfigureAwait(false);

        return new CreateDebtRecordResponse(detail.Record);
    }
}
