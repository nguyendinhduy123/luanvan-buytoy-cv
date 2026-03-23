using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace buytoy.Models
{
    public class BrandModel
    {
        [Key]
        public int Id { get; set; }
        [Required( ErrorMessage = "Yêu cầu nhập tên danh mục")]
        public string Name { get; set; }
        [Required( ErrorMessage = "Yêu cầu nhập mô tả thương hiệu")]
        public string Description { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }
        public string? Image { get; set; }

        [NotMapped]
        public IFormFile? ImageUpload { get; set; }


    }
}
