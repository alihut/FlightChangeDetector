using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightChangeDetector.Domain.Entities
{
    public class Route : BaseEntity
    {
        public int OriginCityId { get; set; }

        public int DestinationCityId { get; set; }

        public DateTime DepartureDate { get; set; }

        public virtual ICollection<Flight> Flights { get; set; }
    }
}
