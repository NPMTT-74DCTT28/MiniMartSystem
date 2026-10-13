namespace MiniMart.Shared.DTOs.ImportReceipts
{
    public class ImportReceiptDto
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }

        public Guid UserId { get; set; }

        public DateTime ImportDate { get; set; }

        public decimal TotalAmount { get; set; }

        public List<ImportReceiptDetailDto> Details { get; set; } = new();
    }
}
