namespace FlightChangeDetector.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public int AgencyId { get; set; }

        public int OriginCityId { get; set; }

        public int DestinationCityId { get; set; }
    }
}
