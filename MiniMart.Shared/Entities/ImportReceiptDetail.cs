using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniMart.Shared.Entities
{
    public class ImportReceiptDetail
    {
        [Key]
        public int Id { get; set; }

        public int ImportReceiptId { get; set; }

        public ImportReceipt ImportReceipt { get; set; } = null!;

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ImportPrice { get; set; }
    }
}
