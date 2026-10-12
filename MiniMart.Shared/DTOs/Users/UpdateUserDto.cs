using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.Users
{
    /// <summary>
    /// DTO dùng khi cập nhật tài khoản nhân viên.
    /// </summary>
    public class UpdateUserDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu mới (dạng thô). Để trống/null nếu không muốn đổi mật khẩu.
        /// </summary>
        [MaxLength(255)]
        public string? Password { get; set; }

        /// <summary>
        /// Vai trò của người dùng: "Admin" hoặc "Staff".
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Staff";

        public bool IsActive { get; set; } = true;
    }
}
