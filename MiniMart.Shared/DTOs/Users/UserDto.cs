using MiniMart.Shared.Enums;

namespace MiniMart.Shared.DTOs.Users
{
    /// <summary>
    /// DTO trả về thông tin tài khoản nhân viên cho Client.
    /// Không chứa PasswordHash để tránh lộ thông tin nhạy cảm.
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò của người dùng (<see cref="UserRole.Admin"/> hoặc <see cref="UserRole.Staff"/>).
        /// </summary>
        public UserRole Role { get; set; } = UserRole.Staff;

        public bool IsActive { get; set; }
    }
}
