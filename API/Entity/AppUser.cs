using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace API.Entity
{
    public class AppUser : IdentityUser<int>
    {
        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        public ICollection<Contact> Contacts { get; set; }
        public ICollection<AppUserRole> UserRoles { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
