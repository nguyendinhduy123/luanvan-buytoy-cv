using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace buytoy.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập đánh giá sản phẩm")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Email")]
        public string Email { get; set; }
        public int Star {  get; set; }

        [ForeignKey("ProductId")]
        // (There's a missing type/property definition here, likely related to a Product object)
        public ProductModel Product { get; set; }
    }
}
