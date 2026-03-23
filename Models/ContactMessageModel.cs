using System;
using System.ComponentModel.DataAnnotations;

namespace buytoy.Models
{
    public class ContactMessageModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
