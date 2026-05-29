using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Sources.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.CreateSource;

/// <summary>Persists a new <see cref="FinSource"/> with audit + history hooks inside one DB transaction.</summary>
public sealed class CreateSourceCommandHandler : IRequestHandler<CreateSourceCommand, CreateSourceResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public CreateSourceCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Creates the finance source row, relying on interceptors for audit/history writes.</summary>
    /// <inheritdoc cref="IRequestHandler{CreateSourceCommand, CreateSourceResponse}.Handle" />
    public async Task<CreateSourceResponse> Handle(CreateSourceCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var nowUser = _currentUser.UserId.Value;
        var sessionId = _currentUser.SessionId!.Value;
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency.Trim().ToUpperInvariant();

        var opening = request.Type != SourceType.CreditCard && request.InitialBalance is { } ib
            ? PFP.Application.Common.CurrencyUnits.FromWhole(ib)
            : 0m;

        var entity = new FinSource
        {            Name = request.Name.Trim(),
            Type = request.Type,
            Balance = opening,
            InitialBalance = request.Type != SourceType.CreditCard ? opening : null,
            Currency = currency,
            CreditLimit = request.CreditLimit,
            StatementDay = request.StatementDay,
            PaymentDueDay = request.PaymentDueDay,
            MinInstallmentAmt = request.MinInstallmentAmt,
            Icon = request.Icon?.Trim(),
            Color = request.Color?.Trim(),
            SortOrder = request.SortOrder ?? 0,
            UpdatedBy = nowUser,
            LastSessionId = sessionId,
        };

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        _db.FinSources.Add(entity);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new CreateSourceResponse(FinSourceDtoMapper.ToDto(entity));
    }
}
