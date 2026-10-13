using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.Orders
{
    public class CreateOrderDetailDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}