using System.ComponentModel.DataAnnotations;

namespace API.Dto
{
    public class RegDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string City { get; set; }
        
        [Required] 
        public string Country { get; set; }

        [Required] public string PhoneNumber { get; set; }
        [Required] public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(25)]
        public string Password { get; set; }
    }
}
