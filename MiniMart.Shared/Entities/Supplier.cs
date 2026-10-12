using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.Entities
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public ICollection<ImportReceipt> ImportReceipts { get; set; } = new List<ImportReceipt>();
    }
}
