using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        public int TotalPoints { get; set; }
    }
}
