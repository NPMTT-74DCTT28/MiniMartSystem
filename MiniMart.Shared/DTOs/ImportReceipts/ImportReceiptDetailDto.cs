namespace MiniMart.Shared.DTOs.ImportReceipts
{
    public class ImportReceiptDetailDto
    {
        public int Id { get; set; }

        public int ImportReceiptId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal ImportPrice { get; set; }
    }
}
