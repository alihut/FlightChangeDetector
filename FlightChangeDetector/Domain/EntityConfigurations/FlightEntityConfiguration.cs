using FlightChangeDetector.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightChangeDetector.Domain.EntityConfigurations
{
    public class FlightEntityConfiguration : BaseEntityConfiguration<Flight>
    {
        public override void Configure(EntityTypeBuilder<Flight> builder)
        {
            base.Configure(builder);

            builder
                .Property(x => x.AirlineId)
                .IsRequired();

            builder
                .Property(x => x.ArrivalTime)
                .IsRequired();

            builder
                .Property(x => x.DepartureTime)
                .IsRequired();

            builder
                .Property(x => x.RouteId)
                .IsRequired();

            builder.HasIndex(x => x.RouteId);
            builder.HasIndex(x => x.AirlineId);

            builder.HasIndex(x => x.ArrivalTime);
            builder.HasIndex(x => x.DepartureTime);

            builder
                .HasOne(p => p.Route)
                .WithMany(c => c.Flights)
                .HasForeignKey(c => c.RouteId);

        }
    }
}
