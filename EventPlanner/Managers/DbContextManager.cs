using EventPlanner.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Managers
{
    public class DbContextManager : DbContext
    {
        public DbSet<User> Users { get; set; } = null;

        public DbContextManager()
        {
            Database.EnsureCreated();
        }
        public override void OnConfiguring(DbContextOptions optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=пароль_от_postgres");
        }
    }
}
