using MiniMart.Client.Forms;
using MiniMart.Shared.DTOs.Users;
using MiniMart.Shared.Enums;

namespace MiniMart.Client.Navigation;

/// <summary>
/// Danh mục tập trung toàn bộ màn hình chức năng của 5 thành viên, dùng làm nguồn dữ liệu
/// cho thanh điều hướng của <see cref="frmMain"/>.
/// Khi một module hoàn thiện, chỉ cần thay <c>FormFactory</c> của mục tương ứng bằng form thật.
/// </summary>
public static class NavigationCatalog
{
    /// <summary>
    /// Các màn hình đã đăng ký, sắp xếp theo thứ tự hiển thị trên thanh điều hướng.
    /// </summary>
    public static IReadOnlyList<NavigationItem> Items { get; } = new List<NavigationItem>
    {
        // Member 1 (Leader) - Hệ thống & thống kê
        Pending(
            key: "dashboard",
            group: "Tổng quan",
            title: "Tổng quan và thống kê",
            description: "Xem nhanh doanh thu và tình hình bán hàng.",
            owner: "Member 1 (Leader)"),
        new(
            Key: "accounts",
            Group: "Hệ thống",
            Title: "Quản lý tài khoản nhân viên",
            Description: "Tạo, cập nhật, vô hiệu hóa tài khoản và phân quyền.",
            FormFactory: () => new frmAccountManager(),
            RequiredRole: UserRole.Admin,
            Owner: "Member 1 (Leader)"),

        // Member 2 - Quản lý Sản phẩm (Catalog)
        new(
            Key: "catalog",
            Group: "Danh mục",
            Title: "Quản lý danh mục và sản phẩm",
            Description: "CRUD danh mục và sản phẩm.",
            FormFactory: () => new frmCatalogManager(),
            RequiredRole: UserRole.Admin,
            Owner: "Member 2"),

        // Member 3 - Quản lý Khách hàng (Customers)
        new NavigationItem(
            Key: "customers",
            Group: "Khách hàng",
            Title: "Quản lý khách hàng",
            Description: "CRUD khách hàng và theo dõi điểm tích lũy/hạng thành viên.",
            FormFactory: () => new frmCustomerManager(),
            Owner: "Member 3"),

        // Member 4 - Bán hàng (POS / Orders)
        Pending(
            key: "pos",
            group: "Bán hàng",
            title: "Bán hàng tại quầy (POS)",
            description: "Chọn sản phẩm, tính tiền và in hóa đơn.",
            owner: "Member 4"),

        // Member 5 - Nhập kho (Inventory / Suppliers)
        new NavigationItem(
            Key: "inventory",
            Group: "Nhập kho",
            Title: "Nhập kho và nhà cung cấp",
            Description: "Quản lý nhà cung cấp và tạo phiếu nhập kho.",
            FormFactory: () => new frmInventoryManager(),
            RequiredRole: UserRole.Admin,
            Owner: "Member 5")
    };

    /// <summary>Lấy các màn hình hiển thị với tài khoản đang đăng nhập, giữ nguyên thứ tự đăng ký.</summary>
    public static IReadOnlyList<NavigationItem> VisibleItems(UserDto? currentUser)
        => Items.Where(item => item.IsVisibleFor(currentUser)).ToList();

    /// <summary>Lấy các màn hình hiển thị với tài khoản đang đăng nhập, đã nhóm theo <see cref="NavigationItem.Group"/>.</summary>
    public static IReadOnlyList<IGrouping<string, NavigationItem>> VisibleGroups(UserDto? currentUser)
        => VisibleItems(currentUser).GroupBy(item => item.Group).ToList();

    /// <summary>
    /// Tạo màn hình giữ chỗ cho module chưa hoàn thiện, giúp thanh điều hướng
    /// hiển thị đủ các màn hình ngay từ đầu.
    /// </summary>
    private static NavigationItem Pending(
        string key,
        string group,
        string title,
        string description,
        string owner,
        UserRole? requiredRole = null)
        => new(
            Key: key,
            Group: group,
            Title: title,
            Description: description,
            FormFactory: () => new frmPlaceholder(title, owner),
            RequiredRole: requiredRole,
            Owner: owner,
            IsReady: false);
}
