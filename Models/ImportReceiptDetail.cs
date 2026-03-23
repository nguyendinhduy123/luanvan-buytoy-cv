namespace buytoy.Models
{
    public class ImportReceiptDetail
    {
        public int Id { get; set; }
        public int ImportReceiptId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public ImportReceiptModel ImportReceipt { get; set; }
        public ProductModel Product { get; set; }
    }

}
