using buytoy.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace buytoy.Models
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
        public string Name { get; set; }
        public string Slug { get; set; }
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
      
        

        public string ShortDescription { get; set; } 
        [Required(ErrorMessage = "The ShortDescription field is required.")]
        [Display(Name = "Short Description")]// Mô tả ngắn nổi bật
        public string Description { get; set; }
        [Required, Range(1, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public DateTime CreatedDate { get; set; }

        public BrandModel Brand { get; set; }
        public CategoryModel Category { get; set; }
        public RatingModel Rating { get; set; }
        public string Image { get; set; } = "noimage.jpg";
        [NotMapped]
        [FileExtension] 
        public IFormFile? ImageUpload { get; set; }
        [Display(Name = "Tuổi tối thiểu")]
        public int MinAge { get; set; }

        [Display(Name = "Tuổi tối đa")]
        public int MaxAge { get; set; }
        public bool IsFeatured { get; set; }

    }
}
