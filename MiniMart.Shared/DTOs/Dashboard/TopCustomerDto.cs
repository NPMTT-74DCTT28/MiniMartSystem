namespace MiniMart.Shared.DTOs.Dashboard
{
    /// <summary>
    /// Số liệu mua hàng của một khách hàng trong kỳ thống kê.
    /// </summary>
    public class TopCustomerDto
    {
        /// <summary>Mã khách hàng.</summary>
        public int CustomerId { get; set; }

        /// <summary>Tên khách hàng.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Số điện thoại khách hàng.</summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>Số hóa đơn khách hàng đã mua trong kỳ.</summary>
        public int OrderCount { get; set; }

        /// <summary>Tổng chi tiêu của khách hàng trong kỳ.</summary>
        public decimal Revenue { get; set; }

        /// <summary>Điểm tích lũy hiện tại của khách hàng.</summary>
        public int TotalPoints { get; set; }
    }
}
