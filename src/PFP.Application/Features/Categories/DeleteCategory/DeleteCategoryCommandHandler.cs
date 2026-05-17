using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.DeleteCategory;

/// <summary>Soft-deletes a category when it is unused and has no active children.</summary>
public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public DeleteCategoryCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Applies soft-delete inside one DB transaction.</summary>
    /// <inheritdoc cref="IRequestHandler{DeleteCategoryCommand, DeleteCategoryResponse}.Handle" />
    public async Task<DeleteCategoryResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinCategories
            .Include(c => c.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Finance category was not found.");

        if (entity.IsSystem)
            throw new BusinessRuleException("Không thể xóa danh mục hệ thống.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(entity.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage finance categories for this module.");

        if (_currentUser.CurrentOrgId is { } orgId && entity.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this finance category.");

        if (await _db.FinTransactions.AnyAsync(t => t.CategoryId == request.Id, cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException("Không thể xóa danh mục đang được sử dụng bởi giao dịch.");

        if (await _db.FinCategories.AnyAsync(c => c.ParentId == request.Id, cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException("Xóa danh mục con trước");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.FinCategories.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new DeleteCategoryResponse(request.Id);
    }
}
