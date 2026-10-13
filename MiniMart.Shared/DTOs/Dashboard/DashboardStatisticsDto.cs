namespace MiniMart.Shared.DTOs.Dashboard
{
    /// <summary>
    /// Toàn bộ số liệu thống kê của màn hình Dashboard trong một khoảng thời gian.
    /// Client không đọc trực tiếp database nên mọi số liệu đều lấy qua một lần gọi
    /// <c>GET api/dashboard</c>.
    /// </summary>
    public class DashboardStatisticsDto
    {
        /// <summary>Ngày bắt đầu của kỳ thống kê (đã chuẩn hóa về phần ngày).</summary>
        public DateTime FromDate { get; set; }

        /// <summary>Ngày kết thúc của kỳ thống kê (tính trọn ngày).</summary>
        public DateTime ToDate { get; set; }

        /// <summary>Các chỉ số tổng hợp hiển thị trên thẻ số liệu.</summary>
        public DashboardSummaryDto Summary { get; set; } = new();

        /// <summary>Doanh thu theo từng ngày trong kỳ, dùng để vẽ biểu đồ cột.</summary>
        public List<RevenueByDayDto> RevenueByDay { get; set; } = new();

        /// <summary>Sản phẩm bán chạy nhất trong kỳ.</summary>
        public List<TopProductDto> TopProducts { get; set; } = new();

        /// <summary>Doanh thu chia theo danh mục sản phẩm.</summary>
        public List<RevenueByCategoryDto> RevenueByCategory { get; set; } = new();

        /// <summary>Doanh thu chia theo nhân viên bán hàng.</summary>
        public List<RevenueByUserDto> RevenueByUser { get; set; } = new();

        /// <summary>Khách hàng mua nhiều nhất trong kỳ.</summary>
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
    }
}
