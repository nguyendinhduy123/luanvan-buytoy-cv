using System.ComponentModel.DataAnnotations;

namespace buytoy.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu tên coupon")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu tên mô tả")]
        public string Description { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateExpired { get; set; }

        [Required(ErrorMessage = "Yêu cầu số lượng coupon")]
        public int Quantity { get; set; }

        public int Status { get; set; }

        // Thêm vào để xử lý giảm giá
        public decimal Value { get; set; }           // 10 = 10%, hoặc 50000 = 50k
        public bool IsPercentage { get; set; }       // true = %, false = số tiền
    }
}
