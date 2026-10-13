namespace MiniMart.Shared.DTOs.Dashboard
{
    /// <summary>
    /// Số liệu bán hàng của một sản phẩm trong kỳ thống kê.
    /// </summary>
    public class TopProductDto
    {
        /// <summary>Mã sản phẩm.</summary>
        public int ProductId { get; set; }

        /// <summary>Tên sản phẩm.</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>Tổng số lượng đã bán trong kỳ.</summary>
        public int QuantitySold { get; set; }

        /// <summary>Doanh thu sản phẩm mang lại trong kỳ.</summary>
        public decimal Revenue { get; set; }
    }
}
