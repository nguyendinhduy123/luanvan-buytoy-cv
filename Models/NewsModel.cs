using System;
using System.ComponentModel.DataAnnotations;

namespace buytoy.Models
{
    public class NewsModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Ngày đăng")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
