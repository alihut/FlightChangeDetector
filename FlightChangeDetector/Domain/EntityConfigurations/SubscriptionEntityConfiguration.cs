using FlightChangeDetector.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightChangeDetector.Domain.EntityConfigurations
{
    public class SubscriptionEntityConfiguration : BaseEntityConfiguration<Subscription>
    {
        public override void Configure(EntityTypeBuilder<Subscription> builder)
        {
            base.Configure(builder);

            builder
                .Property(x => x.AgencyId)
                .IsRequired();

            builder
                .Property(x => x.DestinationCityId)
                .IsRequired();

            builder
                .Property(x => x.OriginCityId)
                .IsRequired();

            builder.HasIndex(x => x.AgencyId);
            builder.HasIndex(x => x.DestinationCityId);
            builder.HasIndex(x => x.OriginCityId);
        }
    }
}
