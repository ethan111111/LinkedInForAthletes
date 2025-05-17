using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using recruitingWebApp.Models;

namespace PostgreSQL.Data
{
    public class AppDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public AppDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to postgres with connection string from app settings
            options.UseNpgsql(Configuration.GetConnectionString("WebApiDatabase"));
        }

        public DbSet<ProfilePic> Images { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Measurments> Measurment { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.ProfileImage)
                .WithOne(p => p.User)
                .HasForeignKey<User>(u => u.ProfilePicId)
                .OnDelete(DeleteBehavior.SetNull); // If ProfilePic is deleted, ProfilePicId in User is set to NULL
        }


    }
}