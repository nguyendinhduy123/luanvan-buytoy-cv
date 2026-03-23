using buytoy.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace buytoy.Models
{
    public class SliderModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu không được bỏ trống tên slider")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu không được bỏ trống mô tả")]
        public string Description { get; set; }

        public int? Status { get; set; }

        public string Image { get; set; }

        [NotMapped]
        [FileExtension] // This might be a custom attribute or from a specific library
        public IFormFile ImageUpload { get; set; }
    }
}
