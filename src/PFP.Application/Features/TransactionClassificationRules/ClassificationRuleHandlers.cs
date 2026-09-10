using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.TransactionClassificationRules;

public sealed class ClassificationRuleHandlers :
    IRequestHandler<ListClassificationRulesQuery, IReadOnlyList<ClassificationRuleDto>>,
    IRequestHandler<CreateClassificationRuleCommand, ClassificationRuleDto>,
    IRequestHandler<UpdateClassificationRuleCommand, ClassificationRuleDto>,
    IRequestHandler<DeleteClassificationRuleCommand, Unit>,
    IRequestHandler<MatchTransactionContentsCommand, IReadOnlyList<ClassificationResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ITransactionClassificationService _classification;

    public ClassificationRuleHandlers(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ITransactionClassificationService classification)
    {
        _db = db;
        _currentUser = currentUser;
        _classification = classification;
    }

    public async Task<IReadOnlyList<ClassificationRuleDto>> Handle(
        ListClassificationRulesQuery request, CancellationToken cancellationToken)
    {
        var userId = RequireUser();
        return await _db.TransactionClassificationRules.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.NormalizedKeyword.Length).ThenBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .Select(x => new ClassificationRuleDto(
                x.Id, x.Keyword, x.CategoryId, x.Category.Name, x.TagId,
                x.Tag == null ? null : x.Tag.Name, x.Tag == null ? null : x.Tag.Color,
                x.IsActive, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ClassificationRuleDto> Handle(
        CreateClassificationRuleCommand request, CancellationToken cancellationToken)
    {
        var userId = RequireUser();
        return await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            await LockUserAsync(userId, ct);
            await ValidateTargetsAsync(request.CategoryId, request.TagId, ct);
            var keyword = request.Keyword.Trim();
            var normalized = TransactionContentMatcher.Normalize(keyword);
            await EnsureUniqueAsync(userId, normalized, null, ct);
            var entity = new TransactionClassificationRule
            {
                UserId = userId,
                Keyword = keyword,
                NormalizedKeyword = normalized,
                CategoryId = request.CategoryId,
                TagId = request.TagId,
                IsActive = request.IsActive,
            };
            _db.TransactionClassificationRules.Add(entity);
            await _db.SaveChangesAsync(ct);
            return await GetDtoAsync(entity.Id, userId, ct);
        }, cancellationToken);
    }

    public async Task<ClassificationRuleDto> Handle(
        UpdateClassificationRuleCommand request, CancellationToken cancellationToken)
    {
        var userId = RequireUser();
        return await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            await LockUserAsync(userId, ct);
            var entity = await _db.TransactionClassificationRules
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, ct)
                ?? throw new NotFoundException("Classification rule was not found.");
            await ValidateTargetsAsync(request.CategoryId, request.TagId, ct);
            var keyword = request.Keyword.Trim();
            var normalized = TransactionContentMatcher.Normalize(keyword);
            await EnsureUniqueAsync(userId, normalized, entity.Id, ct);
            entity.Keyword = keyword;
            entity.NormalizedKeyword = normalized;
            entity.CategoryId = request.CategoryId;
            entity.TagId = request.TagId;
            entity.IsActive = request.IsActive;
            await _db.SaveChangesAsync(ct);
            return await GetDtoAsync(entity.Id, userId, ct);
        }, cancellationToken);
    }

    public async Task<Unit> Handle(DeleteClassificationRuleCommand request, CancellationToken cancellationToken)
    {
        var userId = RequireUser();
        var entity = await _db.TransactionClassificationRules
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Classification rule was not found.");
        _db.TransactionClassificationRules.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<IReadOnlyList<ClassificationResult>> Handle(
        MatchTransactionContentsCommand request, CancellationToken cancellationToken)
    {
        var userId = RequireUser();
        var output = new List<ClassificationResult>(request.Items.Count);
        foreach (var item in request.Items)
        {
            var match = await _classification.MatchAsync(userId, item.Content, cancellationToken);
            output.Add(new(item.Key, match?.RuleId, match?.CategoryId, match?.TagId));
        }
        return output;
    }

    private Guid RequireUser() => _currentUser.UserId
        ?? throw new UnauthorizedAppException("Authentication is required.");

    private async Task LockUserAsync(Guid userId, CancellationToken ct) =>
        _ = await _db.Users.FromSqlInterpolated($"SELECT * FROM users WITH (UPDLOCK, ROWLOCK) WHERE id = {userId}")
            .SingleAsync(ct);

    private async Task ValidateTargetsAsync(Guid categoryId, Guid? tagId, CancellationToken ct)
    {
        var category = await _db.FinCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == categoryId, ct);
        if (category is null || category.Kind != CategoryKind.Expense)
            throw new BusinessRuleException("Classification rules require an active expense category.");
        if (tagId is not null && !await _db.Tags.AsNoTracking().AnyAsync(x => x.Id == tagId, ct))
            throw new BusinessRuleException("The selected tag is not available.");
    }

    private async Task EnsureUniqueAsync(Guid userId, string normalized, Guid? excludingId, CancellationToken ct)
    {
        if (normalized.Length == 0) throw new BusinessRuleException("Keyword cannot be blank.");
        if (await _db.TransactionClassificationRules.AnyAsync(
                x => x.UserId == userId && x.NormalizedKeyword == normalized && x.Id != excludingId, ct))
            throw new BusinessRuleException("An equivalent classification keyword already exists.");
    }

    private async Task<ClassificationRuleDto> GetDtoAsync(Guid id, Guid userId, CancellationToken ct) =>
        await _db.TransactionClassificationRules.AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new ClassificationRuleDto(
                x.Id, x.Keyword, x.CategoryId, x.Category.Name, x.TagId,
                x.Tag == null ? null : x.Tag.Name, x.Tag == null ? null : x.Tag.Color,
                x.IsActive, x.CreatedAt, x.UpdatedAt))
            .SingleAsync(ct);
}
