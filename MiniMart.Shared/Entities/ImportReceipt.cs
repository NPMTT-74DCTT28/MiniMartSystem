using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Shared.Entities
{
    public class ImportReceipt
    {
        [Key]
        public int Id { get; set; }

        public int SupplierId { get; set; }

        public Supplier Supplier { get; set; } = null!;

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public DateTime ImportDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public ICollection<ImportReceiptDetail> Details { get; set; } = new List<ImportReceiptDetail>();
    }
}
