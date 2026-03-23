namespace buytoy.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string AddressDetail { get; set; }

        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
    }

}
