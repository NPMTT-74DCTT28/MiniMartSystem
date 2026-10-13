using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.Orders
{
    public class CreateOrderDto
    {
        [Range(1, int.MaxValue)]
        public int? CustomerId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateOrderDetailDto> Details { get; set; } = new();
    }
}