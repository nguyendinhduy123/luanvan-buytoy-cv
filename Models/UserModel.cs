using System.ComponentModel.DataAnnotations;

namespace buytoy.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        public bool AcceptPolicy { get; set; }

        public bool ReceiveNews { get; set; }
    }
}
