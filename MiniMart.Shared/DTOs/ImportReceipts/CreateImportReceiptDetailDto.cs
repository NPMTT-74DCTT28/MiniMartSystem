namespace MiniMart.Shared.DTOs.ImportReceipts
{
    public class CreateImportReceiptDetailDto
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal ImportPrice { get; set; }
    }
}
