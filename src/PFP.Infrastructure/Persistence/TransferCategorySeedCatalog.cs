namespace PFP.Infrastructure.Persistence;

internal sealed record TransferCategorySeedNode(
    string Code,
    string Name,
    string Icon,
    string Color);

internal static class TransferCategorySeedCatalog
{
    public const string MarkerCode = "sys-tfr-noi-bo";

    public static IReadOnlyList<TransferCategorySeedNode> Items { get; } =
    [
        new("sys-tfr-noi-bo", "Chuyển nội bộ", "🔄", "#0891b2"),
        new("sys-tfr-nap-vi", "Nạp ví", "📱", "#6366f1"),
        new("sys-tfr-rut-tien", "Rút tiền", "🏧", "#64748b"),
        new("sys-tfr-tiet-kiem", "Chuyển tiết kiệm", "🐷", "#10b981"),
        new("sys-tfr-khac", "Chuyển khoản khác", "📦", "#78716c"),
    ];
}
