using FlightChangeDetector.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightChangeDetector.Domain.EntityConfigurations
{
    public class RouteEntityConfiguration : BaseEntityConfiguration<Route>
    {
        public override void Configure(EntityTypeBuilder<Route> builder)
        {
            base.Configure(builder);

            builder
                .Property(x => x.OriginCityId)
                .IsRequired();

            builder
                .Property(x => x.DestinationCityId)
                .IsRequired();

            builder
                .Property(x => x.DepartureDate)
                .IsRequired();

            builder.HasIndex(x => x.OriginCityId);
            builder.HasIndex(x => x.DestinationCityId);
            builder.HasIndex(x => x.DepartureDate);
        }
    }
}
