using PFP.Application.Features.TransactionClassificationRules;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.CreateTransaction;
using PFP.Application.Features.Transactions.ImportTransactions;
using PFP.Domain.Entities;
using PFP.Domain.Enums;
using Xunit;

namespace PFP.IntegrationTests.Finance;

public sealed class TransactionContentMatcherTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rule_validator_rejects_empty_or_whitespace_keyword(string keyword)
    {
        var validator = new CreateClassificationRuleCommandValidator();
        var result = validator.Validate(new CreateClassificationRuleCommand(keyword, Guid.NewGuid(), null));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("BE")]
    [InlineData("BE.COM")]
    [InlineData("Thanh toan BE")]
    [InlineData("BE - Delivery")]
    [InlineData("thanh toán be")]
    public void Boundary_match_accepts_expected_merchant_variants(string content) =>
        Assert.True(TransactionContentMatcher.IsMatch(content, TransactionContentMatcher.Normalize("BE")));

    [Theory]
    [InlineData("NUMBER")]
    [InlineData("BEAUTY")]
    [InlineData("")]
    [InlineData(null)]
    public void Boundary_match_rejects_embedded_or_empty_content(string? content) =>
        Assert.False(TransactionContentMatcher.IsMatch(content, TransactionContentMatcher.Normalize("BE")));

    [Fact]
    public void Normalize_is_case_diacritic_and_whitespace_insensitive() =>
        Assert.Equal("THANH TOAN DE", TransactionContentMatcher.Normalize("  Thanh   toán đê "));

    [Fact]
    public void Longest_keyword_wins_then_created_time_and_id_are_deterministic()
    {
        var older = new TransactionClassificationRule
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Keyword = "BE", NormalizedKeyword = "BE", IsActive = true, CreatedAt = new DateTime(2026, 1, 1),
        };
        var specific = new TransactionClassificationRule
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Keyword = "BE DELIVERY", NormalizedKeyword = "BE DELIVERY", IsActive = true, CreatedAt = new DateTime(2026, 1, 2),
        };
        var inactive = new TransactionClassificationRule
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Keyword = "BE DELIVERY", NormalizedKeyword = "BE DELIVERY", IsActive = false,
        };

        Assert.Same(specific, TransactionContentMatcher.SelectBest([older, inactive, specific], "BE DELIVERY"));
    }

    [Fact]
    public async Task Import_classifier_fills_missing_values_but_preserves_explicit_user_override()
    {
        var category = Guid.NewGuid();
        var tag = Guid.NewGuid();
        var classifier = new TransactionImportClassifier(
            new CurrentUserStub(Guid.NewGuid()),
            new ClassificationStub(new TransactionClassificationMatch(Guid.NewGuid(), category, tag)));
        var missing = Command(categoryId: null, tagIds: null);
        var automatic = await classifier.ApplyAsync(missing, CancellationToken.None);
        Assert.Equal(category, automatic.CategoryId);
        Assert.Equal([tag], automatic.TagIds);

        var manualCategory = Guid.NewGuid();
        var explicitValues = Command(manualCategory, []);
        var preserved = await classifier.ApplyAsync(explicitValues, CancellationToken.None);
        Assert.Equal(manualCategory, preserved.CategoryId);
        Assert.Empty(preserved.TagIds!);
    }

    private static CreateTransactionCommand Command(Guid? categoryId, IReadOnlyList<Guid>? tagIds) => new(
        TransactionType.Direct, 100, Guid.NewGuid(), categoryId, DateOnly.FromDateTime(DateTime.UtcNow),
        null, "BE.COM", null, null, null, null, null, null, null, Guid.NewGuid(), null, tagIds);

    private sealed record CurrentUserStub(Guid? UserId) : ICurrentUserService
    {
        public Guid? SessionId => null;
        public bool IsAuthenticated => UserId is not null;
        public string? IpAddress => null;
        public string? UserAgent => null;
        public string CurrentLocale => "en";
    }

    private sealed class ClassificationStub(TransactionClassificationMatch? match) : ITransactionClassificationService
    {
        public Task<TransactionClassificationMatch?> MatchAsync(Guid userId, string? content, CancellationToken cancellationToken) =>
            Task.FromResult(match);
    }
}
