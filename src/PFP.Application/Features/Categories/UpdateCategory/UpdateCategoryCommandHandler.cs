using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Categories.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.UpdateCategory;

/// <summary>Updates category fields with tree, default-flag, and kind-change safeguards.</summary>
public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public UpdateCategoryCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Applies updates in a single DB transaction.</summary>
    /// <inheritdoc cref="IRequestHandler{UpdateCategoryCommand, UpdateCategoryResponse}.Handle" />
    public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
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
            throw new BusinessRuleException("Không thể chỉnh sửa danh mục hệ thống.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(entity.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage finance categories for this module.");

        if (_currentUser.CurrentOrgId is { } orgId && entity.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this finance category.");

        if (request.Kind != entity.Kind)
        {
            if (await _db.FinTransactions.AnyAsync(t => t.CategoryId == entity.Id, cancellationToken).ConfigureAwait(false))
                throw new BusinessRuleException("Không thể đổi loại danh mục khi đã có giao dịch sử dụng danh mục này.");

            if (await _db.FinCategories.AnyAsync(c => c.ParentId == entity.Id, cancellationToken).ConfigureAwait(false))
                throw new BusinessRuleException("Không thể đổi loại danh mục khi còn danh mục con.");
        }

        FinCategory? parent = null;
        if (request.ParentId is { } parentId)
        {
            if (parentId == entity.Id)
                throw new BusinessRuleException("Danh mục không thể là cha của chính nó.");

            parent = await _db.FinCategories
                .FirstOrDefaultAsync(c => c.Id == parentId && c.SmoduleId == entity.SmoduleId, cancellationToken)
                .ConfigureAwait(false);

            if (parent is null)
                throw new NotFoundException("Parent category was not found.");

            if (parent.Kind != request.Kind)
                throw new BusinessRuleException("Loại danh mục con phải trùng với loại danh mục cha.");

            if (await WouldCreateCycleAsync(entity.Id, parentId, cancellationToken).ConfigureAwait(false))
                throw new BusinessRuleException("Thiết lập cha không hợp lệ (phát hiện vòng cây).");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        if (request.IsDefault)
            await ClearOtherDefaultsAsync(entity.SmoduleId, request.Kind, entity.Id, cancellationToken).ConfigureAwait(false);

        entity.Name = request.Name.Trim();
        entity.Kind = request.Kind;
        entity.ParentId = request.ParentId;
        entity.Icon = request.Icon?.Trim();
        entity.Color = request.Color?.Trim();
        entity.SortOrder = request.SortOrder ?? entity.SortOrder;
        entity.IsDefault = request.IsDefault;
        entity.Depth = parent?.Depth + 1 ?? 0;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new UpdateCategoryResponse(CategoryDtoMapper.ToSingleNode(entity));
    }

    private async Task<bool> WouldCreateCycleAsync(Guid categoryId, Guid newParentId, CancellationToken ct)
    {
        Guid? walk = newParentId;
        var guard = 0;
        while (walk is not null && guard++ < 256)
        {
            if (walk == categoryId)
                return true;
            walk = await _db.FinCategories
                .AsNoTracking()
                .Where(c => c.Id == walk)
                .Select(c => c.ParentId)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        return false;
    }

    private async Task ClearOtherDefaultsAsync(
        Guid smoduleId,
        CategoryKind kind,
        Guid exceptId,
        CancellationToken cancellationToken)
    {
        var rows = await _db.FinCategories
            .Where(c => c.SmoduleId == smoduleId && c.Kind == kind && c.IsDefault && c.Id != exceptId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (var row in rows)
            row.IsDefault = false;
    }
}
