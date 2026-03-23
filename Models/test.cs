using System.ComponentModel.DataAnnotations;

namespace buytoy.Models
{
    public class test
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên không được để trống")]
        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Tên chỉ được chứa chữ cái và số")]

        public string Name { get; set; }
        [Required(ErrorMessage = "create không được để trống")]
        public string Create {  get; set; }
        [Required(ErrorMessage = "delete không được để trống")]
        public string Delete { get; set; }
        [Required(ErrorMessage = "update không được để trống")]
        public string Update { get; set; }
        
    }
}
