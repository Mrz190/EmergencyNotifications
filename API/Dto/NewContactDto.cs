using System.ComponentModel.DataAnnotations;

namespace API.Dto
{
    public class NewContactDto
    {
        [Required] public string Name { get; set; }
        [Required] public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public string CreatedBy { get; set; } 

    }
}
