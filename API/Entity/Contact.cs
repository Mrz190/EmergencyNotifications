using System.ComponentModel.DataAnnotations;

namespace API.Entity
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }
    
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        
    }
}
