namespace buytoy.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal GrandTotal { get; set; }
        public  decimal ShippingCost { get; set; }
        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalTotal => GrandTotal + ShippingCost - DiscountAmount; // Tổng sau giảm
    }
}
