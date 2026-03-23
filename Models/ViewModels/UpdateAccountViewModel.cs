using System.ComponentModel.DataAnnotations;

namespace buytoy.Models.ViewModels
{
    public class UpdateAccountViewModel
    {
        public string Username { get; set; }  // không sửa nhưng vẫn hiển thị
        public string Email { get; set; }     // không sửa nhưng vẫn hiển thị

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
        public string Gender { get; set; }

        public bool ReceiveNews { get; set; }

        // Mật khẩu
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}
