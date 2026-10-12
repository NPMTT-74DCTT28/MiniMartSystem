using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.Auth
{
    /// <summary>
    /// DTO dùng khi Client gửi yêu cầu đăng nhập.
    /// Mật khẩu được gửi dạng thô qua HTTPS, API không bao giờ lưu lại giá trị này.
    /// </summary>
    public class LoginRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;
    }
}
