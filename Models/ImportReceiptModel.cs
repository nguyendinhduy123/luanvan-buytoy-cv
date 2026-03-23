namespace buytoy.Models
{
    public class ImportReceiptModel
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public DateTime ImportDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string Note { get; set; }
        public SupplierModel Supplier { get; set; }
        public List<ImportReceiptDetail> Details { get; set; }
    }

}
