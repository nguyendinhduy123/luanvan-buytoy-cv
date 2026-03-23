using System.ComponentModel.DataAnnotations;

namespace buytoy.Models
{
    public class ProductQuantityModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu không được bỏ trống số lượng sp")]
        public int Quantity { get; set; }

        public int ProductId { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
