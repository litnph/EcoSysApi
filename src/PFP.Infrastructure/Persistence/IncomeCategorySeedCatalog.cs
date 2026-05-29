namespace PFP.Infrastructure.Persistence;

internal sealed record IncomeCategorySeedNode(
    string Code,
    string Name,
    string Icon,
    string Color);

internal static class IncomeCategorySeedCatalog
{
    public const string MarkerCode = "sys-inc-luong";

    public static IReadOnlyList<IncomeCategorySeedNode> Items { get; } =
    [
        new("sys-inc-luong", "Lương", "💰", "#059669"),
        new("sys-inc-freelance", "Freelance", "💼", "#2563eb"),
        new("sys-inc-thuong", "Thưởng", "🎁", "#f59e0b"),
        new("sys-inc-dau-tu", "Đầu tư / Cổ tức", "📈", "#10b981"),
        new("sys-inc-cho-vay", "Thu nợ / Hoàn trả", "🤝", "#0891b2"),
        new("sys-inc-khac", "Thu nhập khác", "📋", "#64748b"),
    ];
}
