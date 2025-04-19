using System;
using System.ComponentModel.DataAnnotations;

namespace API.Entity
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [Required(ErrorMessage = "Имя обязательно.")]
        [MaxLength(100, ErrorMessage = "Имя не может превышать 100 символов.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Номер телефона обязателен.")]
        [Phone(ErrorMessage = "Неверный формат номера телефона.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Электронная почта обязательна.")]
        [EmailAddress(ErrorMessage = "Неверный формат электронной почты.")]
        public string Email { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc); // Гарантируем UTC с правильным Kind

        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; }
    }
}