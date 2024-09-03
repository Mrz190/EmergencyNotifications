using API.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser,
                                                 AppRole,
                                                 int,
                                                 IdentityUserClaim<int>,
                                                 AppUserRole,
                                                 IdentityUserLogin<int>,
                                                 IdentityRoleClaim<int>,
                                                 IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        
        }
        public DbSet<Contact> Contact { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Contact>(op =>
            {
                op.HasIndex(e => e.ContactId);
            });
            builder.Entity<Contact>().ToTable("Contact");




            // -------------------------------- For User Authorization ----------------------------
            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
 
        }
    }
}