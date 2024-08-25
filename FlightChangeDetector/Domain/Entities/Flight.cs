namespace FlightChangeDetector.Domain.Entities
{
    public class Flight : BaseEntity
    {
        public int RouteId { get; set; }

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public int AirlineId { get; set; }

        public virtual Route Route { get; set; }
    }
}
