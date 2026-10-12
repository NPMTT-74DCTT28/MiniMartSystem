using System.Text;

namespace MiniMart.API.Options
{
    /// <summary>
    /// Cấu hình phát hành JWT, đọc từ section <c>Jwt</c> trong <c>appsettings.json</c>.
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        /// <summary>Đơn vị phát hành token.</summary>
        public string Issuer { get; set; } = "MiniMart.API";

        /// <summary>Đối tượng nhận token (WinForms Client).</summary>
        public string Audience { get; set; } = "MiniMart.Client";

        /// <summary>
        /// Khóa bí mật đối xứng dùng để ký token (HMAC-SHA256), tối thiểu 32 byte.
        /// Khi triển khai thật nên nạp từ biến môi trường hoặc user-secrets thay vì file cấu hình.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>Thời gian sống của access token, tính bằng phút.</summary>
        public int ExpiryMinutes { get; set; } = 120;

        /// <summary>
        /// Kiểm tra các giá trị bắt buộc và báo lỗi ngay lúc khởi động thay vì lỗi khó hiểu khi chạy.
        /// </summary>
        public JwtSettings Validate()
        {
            if (string.IsNullOrWhiteSpace(Issuer))
            {
                throw new InvalidOperationException("Jwt:Issuer chưa được cấu hình.");
            }

            if (string.IsNullOrWhiteSpace(Audience))
            {
                throw new InvalidOperationException("Jwt:Audience chưa được cấu hình.");
            }

            if (string.IsNullOrWhiteSpace(Key) || Encoding.UTF8.GetByteCount(Key) < 32)
            {
                throw new InvalidOperationException("Jwt:Key chưa được cấu hình hoặc ngắn hơn 32 byte, không đủ an toàn cho HMAC-SHA256.");
            }

            if (ExpiryMinutes <= 0)
            {
                throw new InvalidOperationException("Jwt:ExpiryMinutes phải là số phút lớn hơn 0.");
            }

            return this;
        }
    }
}
