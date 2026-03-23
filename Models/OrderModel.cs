using System.ComponentModel.DataAnnotations.Schema;

namespace buytoy.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }

        public decimal ShippingCost { get; set; }
        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalPrice { get; set; }          // ✅ Tổng trước giảm
        public decimal FinalTotal { get; set; }          // ✅ Thành tiền cuối cùng
        public string Email { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }

        public int Status { get; set; }
        public string PaymentMethod { get; set; }

        [NotMapped]
        public string StatusName => Status switch
        {
            0 => "Chờ xác nhận",
            1 => "Đang giao",
            2 => "Đã giao",
            3 => "Đã hủy",
            _ => "Không xác định"
        };

        [NotMapped]
        public decimal TotalAmount => TotalPrice - DiscountAmount + ShippingCost;
    }
}
