using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _232252s_Assn2_appsec.Models
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _configuration;

        // Add DbSet<UserActivity> to represent user activities
        public DbSet<UserActivity> UserActivities { get; set; }

        // Constructor to get the configuration
        public AuthDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // OnConfiguring method to set the connection string
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = _configuration.GetConnectionString("AuthConnectionString");
            optionsBuilder.UseSqlServer(connectionString);
        }

        // Optionally, configure the model (e.g., table name or relationships) if necessary
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // You can configure the UserActivity table here if needed (e.g., table name, column names)
            builder.Entity<UserActivity>()
                .HasOne(u => u.User)  // Link to ApplicationUser
                .WithMany()           // You can adjust the relationship if you need to customize it
                .HasForeignKey(ua => ua.UserId);
        }
    }

}
