using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.DeleteSource;

/// <summary>Soft-deletes a finance source when no active <see cref="Domain.Entities.FinTransaction"/> rows reference it.</summary>
public sealed class DeleteSourceCommandHandler : IRequestHandler<DeleteSourceCommand, DeleteSourceResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public DeleteSourceCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Soft-deletes the source when no transactions reference it.</summary>
    /// <inheritdoc cref="IRequestHandler{DeleteSourceCommand, DeleteSourceResponse}.Handle" />
    public async Task<DeleteSourceResponse> Handle(DeleteSourceCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Finance source was not found.");
var hasTransactions = await _db.FinTransactions
            .AnyAsync(
                t => t.SourceId == request.Id || t.DestSourceId == request.Id,
                cancellationToken)
            .ConfigureAwait(false);

        if (hasTransactions)
            throw new BusinessRuleException("Không thể xóa nguồn tài chính đang có giao dịch");

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        _db.FinSources.Remove(entity);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new DeleteSourceResponse(request.Id);
    }
}
