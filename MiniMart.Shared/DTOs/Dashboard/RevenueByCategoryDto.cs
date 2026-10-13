namespace MiniMart.Shared.DTOs.Dashboard
{
    /// <summary>
    /// Doanh thu của một danh mục sản phẩm trong kỳ thống kê.
    /// </summary>
    public class RevenueByCategoryDto
    {
        /// <summary>Mã danh mục.</summary>
        public int CategoryId { get; set; }

        /// <summary>Tên danh mục.</summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>Tổng số lượng sản phẩm thuộc danh mục đã bán trong kỳ.</summary>
        public int QuantitySold { get; set; }

        /// <summary>Doanh thu của danh mục trong kỳ.</summary>
        public decimal Revenue { get; set; }
    }
}
