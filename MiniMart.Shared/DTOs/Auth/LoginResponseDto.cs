using MiniMart.Shared.DTOs.Users;

namespace MiniMart.Shared.DTOs.Auth
{
    /// <summary>
    /// DTO API trả về khi đăng nhập thành công.
    /// Client dùng <see cref="AccessToken"/> cho các request sau và <see cref="User"/> để phân quyền giao diện.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>JWT access token (Bearer) để gắn vào header Authorization của các request tiếp theo.</summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>Thời điểm token hết hạn (UTC).</summary>
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>Thông tin tài khoản đăng nhập, không chứa PasswordHash.</summary>
        public UserDto User { get; set; } = new();
    }
}
