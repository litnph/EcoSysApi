using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
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
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Finance category was not found.");

        if (entity.IsSystem)
            throw new BusinessRuleException("Không thể xóa danh mục hệ thống.");
if (await _db.FinTransactions.AnyAsync(t => t.CategoryId == request.Id, cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException("Không thể xóa danh mục đang được sử dụng bởi giao dịch.");

        if (await _db.FinCategories.AnyAsync(c => c.ParentId == request.Id, cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException("Xóa danh mục con trước");

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        _db.FinCategories.Remove(entity);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new DeleteCategoryResponse(request.Id);
    }
}
