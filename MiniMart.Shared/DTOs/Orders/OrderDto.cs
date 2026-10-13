namespace MiniMart.Shared.DTOs.Orders
{
    public class OrderDto
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }

        public Guid UserId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public List<OrderDetailDto> Details { get; set; } = new();
    }
}