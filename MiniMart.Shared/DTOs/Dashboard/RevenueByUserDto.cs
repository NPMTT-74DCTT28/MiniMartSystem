namespace MiniMart.Shared.DTOs.Dashboard
{
    /// <summary>
    /// Doanh thu của một nhân viên bán hàng trong kỳ thống kê.
    /// </summary>
    public class RevenueByUserDto
    {
        /// <summary>Id tài khoản nhân viên.</summary>
        public Guid UserId { get; set; }

        /// <summary>Tên đăng nhập của nhân viên.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Số hóa đơn nhân viên đã tạo trong kỳ.</summary>
        public int OrderCount { get; set; }

        /// <summary>Doanh thu nhân viên mang lại trong kỳ.</summary>
        public decimal Revenue { get; set; }
    }
}
