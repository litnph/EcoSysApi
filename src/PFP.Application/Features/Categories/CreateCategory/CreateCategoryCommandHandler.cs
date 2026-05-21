using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Categories.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.CreateCategory;

/// <summary>Creates a <see cref="FinCategory"/> and clears competing default flags when needed.</summary>
public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public CreateCategoryCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Persists the category inside one DB transaction.</summary>
    /// <inheritdoc cref="IRequestHandler{CreateCategoryCommand, CreateCategoryResponse}.Handle" />
    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
FinCategory? parent = null;
        if (request.ParentId is { } parentId)
        {
            parent = await _db.FinCategories
                .FirstOrDefaultAsync(c => c.Id == parentId, cancellationToken)
                .ConfigureAwait(false);

            if (parent is null)
                throw new NotFoundException("Parent category was not found.");

            if (parent.Kind != request.Kind)
                throw new BusinessRuleException("Loại danh mục con phải trùng với loại danh mục cha.");
        }

        var depth = parent?.Depth + 1 ?? 0;
        var entity = new FinCategory
        {            Name = request.Name.Trim(),
            Code = CategoryCodeFactory.NewUniqueCode(),
            Kind = request.Kind,
            ParentId = request.ParentId,
            Icon = request.Icon?.Trim(),
            Color = request.Color?.Trim(),
            SortOrder = request.SortOrder ?? 0,
            IsDefault = request.IsDefault,
            Depth = depth,
            Path = null,
            IsSystem = false,
        };

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        if (request.IsDefault)
            await ClearOtherDefaultsAsync(request.Kind, exceptId: null, cancellationToken).ConfigureAwait(false);

        _db.FinCategories.Add(entity);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new CreateCategoryResponse(CategoryDtoMapper.ToSingleNode(entity));
    }

    private async Task ClearOtherDefaultsAsync(
        CategoryKind kind,
        Guid? exceptId,
        CancellationToken cancellationToken)
    {
        IQueryable<FinCategory> query = _db.FinCategories;
        if (exceptId is { } id)
            query = query.Where(c => c.Id != id);

        var rows = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var row in rows)
            row.IsDefault = false;
    }
}
