namespace MiniMart.Shared.DTOs.Dashboard
{
    /// <summary>
    /// Doanh thu của một ngày trong kỳ thống kê, dùng để vẽ biểu đồ doanh thu theo ngày.
    /// </summary>
    public class RevenueByDayDto
    {
        /// <summary>Ngày thống kê (theo UTC).</summary>
        public DateTime Date { get; set; }

        /// <summary>Số hóa đơn trong ngày.</summary>
        public int OrderCount { get; set; }

        /// <summary>Doanh thu trong ngày.</summary>
        public decimal Revenue { get; set; }
    }
}
