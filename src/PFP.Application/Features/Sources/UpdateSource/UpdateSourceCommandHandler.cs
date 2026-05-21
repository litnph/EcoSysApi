using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Sources.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.UpdateSource;

/// <summary>Applies updates to a <see cref="Domain.Entities.FinSource"/>; version + history rows are emitted by interceptors.</summary>
public sealed class UpdateSourceCommandHandler : IRequestHandler<UpdateSourceCommand, UpdateSourceResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public UpdateSourceCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Mutates editable columns on a source; version and <c>FIN_SOURCES_HISTORY</c> rows are handled by EF interceptors.</summary>
    /// <inheritdoc cref="IRequestHandler{UpdateSourceCommand, UpdateSourceResponse}.Handle" />
    public async Task<UpdateSourceResponse> Handle(UpdateSourceCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Finance source was not found.");
var nowUser = _currentUser.UserId.Value;
        var sessionId = _currentUser.SessionId!.Value;
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? entity.Currency : request.Currency.Trim().ToUpperInvariant();

        entity.Name = request.Name.Trim();
        entity.Type = request.Type;
        entity.Currency = currency;
        entity.Icon = request.Icon?.Trim();
        entity.Color = request.Color?.Trim();
        entity.SortOrder = request.SortOrder ?? entity.SortOrder;
        entity.UpdatedBy = nowUser;
        entity.LastSessionId = sessionId;

        if (request.Type == SourceType.CreditCard)
        {
            entity.CreditLimit = request.CreditLimit;
            entity.StatementDay = request.StatementDay;
            entity.PaymentDueDay = request.PaymentDueDay;
            entity.MinInstallmentAmt = request.MinInstallmentAmt;
        }
        else
        {
            entity.CreditLimit = null;
            entity.StatementDay = null;
            entity.PaymentDueDay = null;
            entity.MinInstallmentAmt = null;
        }

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new UpdateSourceResponse(FinSourceDtoMapper.ToDto(entity));
    }
}
