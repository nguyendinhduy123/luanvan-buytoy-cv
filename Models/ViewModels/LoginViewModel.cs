using System.ComponentModel.DataAnnotations;

namespace buytoy.Models.ViewModels
{
    public class LoginViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Làm ơn nhập UserName")]
        public string Username { get; set; }
        
        [DataType(DataType.Password), Required(ErrorMessage = "Làm ơn nhập password")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
