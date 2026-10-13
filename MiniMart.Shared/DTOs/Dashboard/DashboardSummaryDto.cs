namespace MiniMart.Shared.DTOs.Dashboard
{
    /// <summary>
    /// Các chỉ số tổng hợp hiển thị trên thẻ số liệu của Dashboard.
    /// </summary>
    /// <remarks>
    /// Các chỉ số doanh thu và nhập kho được tính theo kỳ thống kê đã chọn, còn các chỉ số
    /// đếm (sản phẩm, khách hàng, nhân viên) là số liệu hiện tại của toàn hệ thống.
    /// </remarks>
    public class DashboardSummaryDto
    {
        /// <summary>Tổng doanh thu bán hàng trong kỳ.</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Số hóa đơn đã tạo trong kỳ.</summary>
        public int OrderCount { get; set; }

        /// <summary>Doanh thu trung bình trên mỗi hóa đơn trong kỳ.</summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>Tổng giá trị nhập kho trong kỳ.</summary>
        public decimal TotalImportAmount { get; set; }

        /// <summary>Số phiếu nhập kho đã tạo trong kỳ.</summary>
        public int ImportReceiptCount { get; set; }

        /// <summary>Tổng số sản phẩm đang quản lý.</summary>
        public int TotalProducts { get; set; }

        /// <summary>Số sản phẩm có tồn kho ở mức thấp (cần nhập thêm).</summary>
        public int LowStockProductCount { get; set; }

        /// <summary>Tổng số khách hàng.</summary>
        public int TotalCustomers { get; set; }

        /// <summary>Tổng số tài khoản nhân viên.</summary>
        public int TotalUsers { get; set; }

        /// <summary>Số sản phẩm bán ra trong kỳ (tổng số lượng trên mọi hóa đơn).</summary>
        public int TotalQuantitySold { get; set; }
    }
}
