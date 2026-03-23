using System.ComponentModel.DataAnnotations;

namespace buytoy.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductModel ProductDetails { get; set; }

        // Đây là thuộc tính dùng để binding khi submit đánh giá
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập đánh giá sản phẩm")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public int? Star { get; set; }
        public double StarAverage { get; set; }
        public int TotalRatings { get; set; }

        public List<RatingModel> Ratings { get; set; }
        public List<string> ImageList { get; set; }
    }
}
