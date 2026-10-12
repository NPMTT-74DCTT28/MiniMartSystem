using MiniMart.Shared.DTOs.Users;
using MiniMart.Shared.Enums;

namespace MiniMart.Client.Navigation;

/// <summary>
/// Mô tả một màn hình chức năng có thể điều hướng tới từ form chung <c>frmMain</c>.
/// </summary>
/// <param name="Key">Khóa duy nhất dùng để định danh màn hình trong quá trình điều hướng.</param>
/// <param name="Group">Nhóm hiển thị trên thanh điều hướng (VD: "Bán hàng").</param>
/// <param name="Title">Tên màn hình hiển thị cho người dùng.</param>
/// <param name="Description">Mô tả ngắn gọn chức năng của màn hình.</param>
/// <param name="FormFactory">Hàm tạo form của màn hình, chỉ được gọi ở lần điều hướng đầu tiên.</param>
/// <param name="RequiredRole">
/// Vai trò được phép nhìn thấy màn hình; <c>null</c> nghĩa là mọi vai trò đều thấy.
/// </param>
/// <param name="Owner">Thành viên phụ trách module, dùng cho thông báo khi màn hình chưa hoàn thiện.</param>
/// <param name="IsReady"><c>true</c> khi màn hình đã hoàn thiện, <c>false</c> khi mới là màn hình giữ chỗ.</param>
public sealed record NavigationItem(
    string Key,
    string Group,
    string Title,
    string Description,
    Func<Form> FormFactory,
    UserRole? RequiredRole = null,
    string? Owner = null,
    bool IsReady = true)
{
    /// <summary>
    /// Kiểm tra màn hình có hiển thị với tài khoản đang đăng nhập hay không.
    /// Khi chưa có thông tin đăng nhập (<paramref name="currentUser"/> là <c>null</c>),
    /// toàn bộ màn hình được hiển thị để phục vụ quá trình phát triển.
    /// </summary>
    public bool IsVisibleFor(UserDto? currentUser)
        => RequiredRole is null || currentUser is null || currentUser.Role == RequiredRole;
}
