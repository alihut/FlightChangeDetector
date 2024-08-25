using FlightChangeDetector.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FlightChangeDetector.Domain
{
    public class MainDbContext : DbContext
    {
        public DbSet<Flight> Flights { get; set; }

        public DbSet<Route> Routes { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
