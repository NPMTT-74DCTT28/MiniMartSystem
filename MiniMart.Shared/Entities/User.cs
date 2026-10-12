using System.ComponentModel.DataAnnotations;
using MiniMart.Shared.Enums;

namespace MiniMart.Shared.Entities
{
    /// <summary>
    /// Tài khoản nhân viên dùng để đăng nhập hệ thống và phân quyền cơ bản.
    /// </summary>
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò của người dùng: "Admin" hoặc "Staff".
        /// </summary>
        [Required]
        public UserRole Role { get; set; } = UserRole.Staff;

        public bool IsActive { get; set; } = true;
    }
}
