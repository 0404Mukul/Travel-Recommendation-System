using AuthAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<SharedLink> SharedLinks
        {
            get;
            set;
        }

        public DbSet<Itinerary> Itineraries { get; set; }
    }
}