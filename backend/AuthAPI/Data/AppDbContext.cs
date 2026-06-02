using Microsoft.EntityFrameworkCore;

using AuthAPI.Entities;

namespace AuthAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options
        ) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}