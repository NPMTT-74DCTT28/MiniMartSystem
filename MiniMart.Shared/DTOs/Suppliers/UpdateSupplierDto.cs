using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.Suppliers
{
    public class UpdateSupplierDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;
    }
}
