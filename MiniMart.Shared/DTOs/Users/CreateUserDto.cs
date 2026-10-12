using System.ComponentModel.DataAnnotations;
using MiniMart.Shared.Enums;

namespace MiniMart.Shared.DTOs.Users
{
    /// <summary>
    /// DTO dùng khi tạo mới tài khoản nhân viên.
    /// Client gửi mật khẩu dạng thô, API chịu trách nhiệm băm (hash) trước khi lưu vào PasswordHash.
    /// </summary>
    public class CreateUserDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò của người dùng (<see cref="UserRole.Admin"/> hoặc <see cref="UserRole.Staff"/>).
        /// Mặc định là <see cref="UserRole.Staff"/>.
        /// </summary>
        [Required]
        public UserRole Role { get; set; } = UserRole.Staff;

        public bool IsActive { get; set; } = true;
    }
}
