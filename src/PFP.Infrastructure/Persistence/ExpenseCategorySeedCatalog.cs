using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence;

internal sealed record ExpenseCategorySeedNode(
    string Code,
    string Name,
    string Icon,
    string Color,
    CategoryNecessityLevel? NecessityLevel,
    IReadOnlyList<ExpenseCategorySeedNode> Children);

/// <summary>Cây danh mục chi tiêu chuẩn (12 nhóm gốc + danh mục con).</summary>
internal static class ExpenseCategorySeedCatalog
{
    public const string MarkerCode = "sys-exp-nha-o";

    public static IReadOnlyList<ExpenseCategorySeedNode> Roots { get; } =
    [
        Group("sys-exp-nha-o", "Nhà ở", "🏠", "#6366f1",
        [
            Child("sys-exp-nha-o-tien-nha", "Tiền nhà", "🏘️", "#6366f1", CategoryNecessityLevel.Needs),
            Child("sys-exp-nha-o-tien-dien", "Tiền điện", "💡", "#eab308", CategoryNecessityLevel.Needs),
            Child("sys-exp-nha-o-tien-nuoc", "Tiền nước", "💧", "#0ea5e9", CategoryNecessityLevel.Needs),
            Child("sys-exp-nha-o-tien-mang", "Tiền mạng", "📶", "#2563eb", CategoryNecessityLevel.Needs),
            Child("sys-exp-nha-o-phi-quan-ly", "Phí quản lý", "🏢", "#64748b", CategoryNecessityLevel.Needs),
            Child("sys-exp-nha-o-do-gia-dung", "Đồ gia dụng", "🛋️", "#8b5cf6", CategoryNecessityLevel.Flexible),
            Child("sys-exp-nha-o-sua-chua", "Sửa chữa nhà cửa", "🔧", "#78716c", CategoryNecessityLevel.Flexible),
        ]),
        Group("sys-exp-an-uong", "Ăn uống", "🍽️", "#f97316",
        [
            Child("sys-exp-an-uong-di-cho", "Đi chợ", "🛒", "#10b981", CategoryNecessityLevel.Needs),
            Child("sys-exp-an-uong-an-ngoai", "Ăn ngoài", "🍜", "#f97316", CategoryNecessityLevel.Flexible),
            Child("sys-exp-an-uong-cafe", "Cafe / Trà sữa", "☕", "#78716c", CategoryNecessityLevel.Flexible),
            Child("sys-exp-an-uong-an-nhau", "Ăn nhậu", "🍻", "#ef4444", CategoryNecessityLevel.Waste),
        ]),
        Group("sys-exp-di-chuyen", "Di chuyển", "🚗", "#0ea5e9",
        [
            Child("sys-exp-di-chuyen-xang", "Xăng xe", "⛽", "#f97316", CategoryNecessityLevel.Needs),
            Child("sys-exp-di-chuyen-gui-xe", "Gửi xe", "🅿️", "#64748b", CategoryNecessityLevel.Needs),
            Child("sys-exp-di-chuyen-taxi", "Taxi / Grab", "🚕", "#0ea5e9", CategoryNecessityLevel.Flexible),
            Child("sys-exp-di-chuyen-bao-duong", "Bảo dưỡng xe", "🔩", "#2563eb", CategoryNecessityLevel.Flexible),
            Child("sys-exp-di-chuyen-sua-xe", "Sửa xe", "🛠️", "#78716c", CategoryNecessityLevel.Needs),
        ]),
        Group("sys-exp-suc-khoe", "Sức khỏe", "💊", "#10b981",
        [
            Child("sys-exp-suc-khoe-thuoc", "Thuốc men", "💊", "#10b981", CategoryNecessityLevel.Needs),
            Child("sys-exp-suc-khoe-kham", "Khám bệnh", "🏥", "#059669", CategoryNecessityLevel.Needs),
            Child("sys-exp-suc-khoe-gym", "Thể thao / Gym", "🏋️", "#14b8a6", CategoryNecessityLevel.Flexible),
            Child("sys-exp-suc-khoe-skincare", "Skincare / Chăm sóc cá nhân", "🧴", "#ec4899", CategoryNecessityLevel.Flexible),
        ]),
        Group("sys-exp-mua-sam", "Mua sắm", "🛍️", "#ec4899",
        [
            Child("sys-exp-mua-sam-thoi-trang", "Thời trang", "👕", "#ec4899", CategoryNecessityLevel.Wants),
            Child("sys-exp-mua-sam-cong-nghe", "Đồ công nghệ", "💻", "#6366f1", CategoryNecessityLevel.Wants),
            Child("sys-exp-mua-sam-phu-kien", "Phụ kiện", "⌚", "#8b5cf6", CategoryNecessityLevel.Flexible),
            Child("sys-exp-mua-sam-noi-that", "Nội thất / Decor", "🪴", "#84cc16", CategoryNecessityLevel.Wants),
            Child("sys-exp-mua-sam-khac", "Mua sắm khác", "🎁", "#f59e0b", CategoryNecessityLevel.Wants),
        ]),
        Group("sys-exp-giao-duc", "Giáo dục", "📚", "#2563eb",
        [
            Child("sys-exp-giao-duc-hoc-phi", "Tiền học", "🎓", "#2563eb", CategoryNecessityLevel.Needs),
            Child("sys-exp-giao-duc-sach", "Sách / Tài liệu", "📖", "#3b82f6", CategoryNecessityLevel.Flexible),
            Child("sys-exp-giao-duc-khoa-hoc", "Khóa học online", "🖥️", "#0891b2", CategoryNecessityLevel.Flexible),
            Child("sys-exp-giao-duc-chung-chi", "Chứng chỉ / Thi cử", "📜", "#1d4ed8", CategoryNecessityLevel.Needs),
        ]),
        Group("sys-exp-giai-tri", "Giải trí", "🎬", "#8b5cf6",
        [
            Child("sys-exp-giai-tri-phim", "Xem phim", "🎬", "#8b5cf6", CategoryNecessityLevel.Wants),
            Child("sys-exp-giai-tri-game", "Game", "🎮", "#6366f1", CategoryNecessityLevel.Wants),
            Child("sys-exp-giai-tri-du-lich", "Du lịch", "✈️", "#0ea5e9", CategoryNecessityLevel.Wants),
            Child("sys-exp-giai-tri-hobbies", "Hobbies", "🎨", "#ec4899", CategoryNecessityLevel.Flexible),
            Child("sys-exp-giai-tri-khac", "Hoạt động giải trí khác", "🎪", "#a855f7", CategoryNecessityLevel.Wants),
        ]),
        Group("sys-exp-digital", "Digital Services", "💻", "#0891b2",
        [
            Child("sys-exp-digital-ai", "AI Tools", "🤖", "#6366f1", CategoryNecessityLevel.Flexible),
            Child("sys-exp-digital-license", "Software License", "📋", "#2563eb", CategoryNecessityLevel.Needs),
            Child("sys-exp-digital-cloud", "Cloud Storage", "☁️", "#0ea5e9", CategoryNecessityLevel.Flexible),
            Child("sys-exp-digital-streaming", "Streaming Subscription", "📺", "#8b5cf6", CategoryNecessityLevel.Wants),
            Child("sys-exp-digital-hosting", "Domain / Hosting", "🌐", "#0891b2", CategoryNecessityLevel.Needs),
        ]),
        Group("sys-exp-xa-hoi", "Xã hội / Quan hệ", "🤝", "#f59e0b",
        [
            Child("sys-exp-xa-hoi-tiec-cuoi", "Tiệc cưới", "💒", "#ec4899", CategoryNecessityLevel.Flexible),
            Child("sys-exp-xa-hoi-sinh-nhat", "Sinh nhật", "🎂", "#f97316", CategoryNecessityLevel.Flexible),
            Child("sys-exp-xa-hoi-di-tiec", "Đi tiệc", "🥂", "#eab308", CategoryNecessityLevel.Wants),
            Child("sys-exp-xa-hoi-qua-tang", "Quà tặng", "🎁", "#f59e0b", CategoryNecessityLevel.Flexible),
            Child("sys-exp-xa-hoi-tham-hoi", "Thăm hỏi", "💐", "#10b981", CategoryNecessityLevel.Needs),
        ]),
        Group("sys-exp-dong-gop", "Đóng góp", "❤️", "#ef4444",
        [
            Child("sys-exp-dong-gop-tu-thien", "Từ thiện", "🤲", "#ef4444", CategoryNecessityLevel.Wants),
            Child("sys-exp-dong-gop-ung-ho", "Ủng hộ", "💝", "#f97316", CategoryNecessityLevel.Wants),
            Child("sys-exp-dong-gop-cung-duong", "Cúng dường", "🪷", "#eab308", CategoryNecessityLevel.Wants),
            Child("sys-exp-dong-gop-ho-tro", "Hỗ trợ gia đình", "👨‍👩‍👧", "#10b981", CategoryNecessityLevel.Needs),
        ]),
        Group("sys-exp-tai-chinh", "Tài chính", "💰", "#059669",
        [
            Child("sys-exp-tai-chinh-tiet-kiem", "Tiết kiệm", "🐷", "#10b981", CategoryNecessityLevel.Needs),
            Child("sys-exp-tai-chinh-dau-tu", "Đầu tư", "📈", "#2563eb", CategoryNecessityLevel.Flexible),
            Child("sys-exp-tai-chinh-tra-no", "Trả nợ", "💳", "#059669", CategoryNecessityLevel.Needs),
            Child("sys-exp-tai-chinh-bao-hiem", "Bảo hiểm", "🛡️", "#0891b2", CategoryNecessityLevel.Needs),
            Child("sys-exp-tai-chinh-phi-ngan-hang", "Phí ngân hàng", "🏦", "#64748b", CategoryNecessityLevel.Needs),
            Child("sys-exp-tai-chinh-lai-vay", "Lãi vay", "📉", "#ef4444", CategoryNecessityLevel.Waste),
        ]),
        Group("sys-exp-khac", "Khác", "📦", "#64748b",
        [
            Child("sys-exp-khac-phat-sinh", "Chi phí phát sinh", "⚡", "#78716c", CategoryNecessityLevel.Flexible),
            Child("sys-exp-khac-phat", "Phạt / Late fee", "⚠️", "#ef4444", CategoryNecessityLevel.Waste),
            Child("sys-exp-khac-mat-do", "Mất đồ", "❓", "#f97316", CategoryNecessityLevel.Waste),
            Child("sys-exp-khac-chi-phi", "Chi phí khác", "📋", "#64748b", CategoryNecessityLevel.Flexible),
        ]),
    ];

    public static IEnumerable<ExpenseCategorySeedNode> Flatten()
    {
        foreach (var root in Roots)
        {
            yield return root;
            foreach (var child in root.Children)
                yield return child;
        }
    }

    private static ExpenseCategorySeedNode Group(
        string code,
        string name,
        string icon,
        string color,
        ExpenseCategorySeedNode[] children) =>
        new(code, name, icon, color, null, children);

    private static ExpenseCategorySeedNode Child(
        string code,
        string name,
        string icon,
        string color,
        CategoryNecessityLevel necessityLevel) =>
        new(code, name, icon, color, necessityLevel, []);
}
