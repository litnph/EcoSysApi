using MediatR;

namespace PFP.Application.Features.TransactionClassificationRules;

public sealed record ClassificationRuleDto(
    Guid Id,
    string Keyword,
    Guid CategoryId,
    string CategoryName,
    Guid? TagId,
    string? TagName,
    string? TagColor,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ListClassificationRulesQuery : IRequest<IReadOnlyList<ClassificationRuleDto>>;
public sealed record CreateClassificationRuleCommand(
    string Keyword, Guid CategoryId, Guid? TagId, bool IsActive = true) : IRequest<ClassificationRuleDto>;
public sealed record UpdateClassificationRuleCommand(
    Guid Id, string Keyword, Guid CategoryId, Guid? TagId, bool IsActive) : IRequest<ClassificationRuleDto>;
public sealed record DeleteClassificationRuleCommand(Guid Id) : IRequest<Unit>;
public sealed record ClassificationInput(string Key, string? Content);
public sealed record ClassificationResult(string Key, Guid? RuleId, Guid? CategoryId, Guid? TagId);
public sealed record MatchTransactionContentsCommand(
    IReadOnlyList<ClassificationInput> Items) : IRequest<IReadOnlyList<ClassificationResult>>;
