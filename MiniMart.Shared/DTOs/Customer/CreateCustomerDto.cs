using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.Customer
{
    public class CreateCustomerDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
