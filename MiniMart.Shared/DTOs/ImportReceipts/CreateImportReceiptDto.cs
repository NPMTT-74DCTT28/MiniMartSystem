using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.ImportReceipts
{
    public class CreateImportReceiptDto
    {
        public int SupplierId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateImportReceiptDetailDto> Details { get; set; } = new();
    }
}
