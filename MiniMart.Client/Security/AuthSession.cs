using MiniMart.Shared.DTOs.Auth;
using MiniMart.Shared.DTOs.Users;
using MiniMart.Shared.Enums;

namespace MiniMart.Client.Security;

/// <summary>
/// Lưu phiên đăng nhập hiện tại của ứng dụng Client (access token + tài khoản).
/// </summary>
/// <remarks>
/// <para>
/// Đây là nơi duy nhất giữ access token trong Client. <see cref="HttpClients.BaseHttpClient"/>
/// đọc token từ đây ở mỗi request nên không cần truyền token thủ công vào từng lời gọi API.
/// </para>
/// <para>
/// Khi đăng nhập/đăng xuất, sự kiện <see cref="Changed"/> được phát sinh để các màn hình
/// cập nhật lại trạng thái hiển thị (VD: thanh điều hướng của <c>frmMain</c>).
/// </para>
/// </remarks>
public static class AuthSession
{
    /// <summary>Phiên đăng nhập hiện tại, hoặc <c>null</c> khi chưa đăng nhập.</summary>
    public static LoginResponseDto? Current { get; private set; }

    /// <summary>Access token dùng cho header <c>Authorization: Bearer</c>.</summary>
    public static string? AccessToken => Current?.AccessToken;

    /// <summary>Tài khoản đang đăng nhập, hoặc <c>null</c> khi chưa đăng nhập.</summary>
    public static UserDto? CurrentUser => Current?.User;

    /// <summary><c>true</c> khi đã có phiên đăng nhập hợp lệ về mặt dữ liệu.</summary>
    public static bool IsAuthenticated => Current is not null;

    /// <summary>Tài khoản đang đăng nhập có phải quản trị viên hay không.</summary>
    public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    /// <summary>Phát sinh mỗi khi phiên đăng nhập thay đổi (đăng nhập hoặc đăng xuất).</summary>
    public static event EventHandler? Changed;

    /// <summary>Bắt đầu phiên đăng nhập mới từ kết quả trả về của <c>POST api/auth/login</c>.</summary>
    public static void SignIn(LoginResponseDto session)
    {
        ArgumentNullException.ThrowIfNull(session);

        Current = session;
        Changed?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>Kết thúc phiên đăng nhập hiện tại (xóa access token khỏi bộ nhớ).</summary>
    public static void SignOut()
    {
        Current = null;
        Changed?.Invoke(null, EventArgs.Empty);
    }
}
