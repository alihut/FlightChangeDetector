using FlightChangeDetector.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightChangeDetector.Domain
{
    public class MainDbContext : DbContext
    {
        public DbSet<Flight> Flights { get; set; }

        public DbSet<Route> Routes { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }
    }
}
