using Microsoft.AspNetCore.Identity;

namespace buytoy.Models
{
    public class AppUserModel : IdentityUser
    {
        public string FirstName { get; set; }         // Tên
        public string LastName { get; set; }          // Họ
        public string Gender { get; set; }            // Giới tính: Nam/Nữ/Khác
        public string Occupation { get; set; }        // Nghề nghiệp (có thể giữ lại nếu dùng)
        public string RoleId { get; set; }            // Vai trò (nếu dùng quản trị)
        public string Token { get; set; }             // Token cho quên mật khẩu, xác thực,...
        public bool ReceiveNews { get; set; }         
        public string? Avatar { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
    }
}
