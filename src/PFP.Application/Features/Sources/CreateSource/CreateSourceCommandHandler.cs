using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
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

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage finance sources for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var nowUser = _currentUser.UserId.Value;
        var sessionId = _currentUser.SessionId!.Value;
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency.Trim().ToUpperInvariant();

        var entity = new FinSource
        {
            SmoduleId = request.SmoduleId,
            Name = request.Name.Trim(),
            Type = request.Type,
            Balance = 0,
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

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.FinSources.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new CreateSourceResponse(FinSourceDtoMapper.ToDto(entity));
    }
}
