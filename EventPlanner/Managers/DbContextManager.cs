using EventPlanner.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Managers
{
    public class DbContextManager : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventTask> EventTasks { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<TaskState> TaskStates { get; set; }
        public DbSet<UserEventTask> UserEventTasks { get; set; }

        public DbContextManager()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=event_planner;Username=event_planner;Password=qwerty");
        }
    }
}
