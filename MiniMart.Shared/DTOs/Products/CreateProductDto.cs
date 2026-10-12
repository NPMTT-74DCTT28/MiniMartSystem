using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.Products
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
