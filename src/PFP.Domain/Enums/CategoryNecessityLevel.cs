namespace PFP.Domain.Enums;

/// <summary>
/// Mức độ cần thiết của danh mục con (chi tiêu) — chỉ áp dụng khi <c>ParentId</c> không null.
/// </summary>
public enum CategoryNecessityLevel
{
    /// <summary><c>needs</c> — Rất cần thiết.</summary>
    Needs = 1,

    /// <summary><c>flexible</c> — Cần thiết nhưng linh hoạt.</summary>
    Flexible = 2,

    /// <summary><c>wants</c> — Tùy chọn / nâng cao chất lượng sống.</summary>
    Wants = 3,

    /// <summary><c>waste</c> — Lãng phí / bốc đồng.</summary>
    Waste = 4,
}
