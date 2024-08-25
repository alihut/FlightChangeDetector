using FlightChangeDetector.Domain;
using FlightChangeDetector.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightChangeDetector.Services
{
    public class FlightChangeDetectorService : IFlightChangeDetectorService
    {
        private readonly MainDbContext _mainDbContext;

        public FlightChangeDetectorService(MainDbContext mainDbContext)
        {
            _mainDbContext = mainDbContext;
        }
        private const int Tolerance = 30;

        private const int DayCount = 7;

        public async Task<List<FlightChangeResult>> DetectFlightChangesAsync(DateTime startDate, DateTime endDate, int agencyId, CancellationToken ct = default)
        {
            var flightChangeResultList = await _mainDbContext.Subscriptions
                .Where(s => s.AgencyId == agencyId)
                .Join(_mainDbContext.Routes,
                    subscription => new { subscription.OriginCityId, subscription.DestinationCityId },
                    route => new { route.OriginCityId, route.DestinationCityId },
                    (subscription, route) => new { Subscription = subscription, Route = route })
                .Join(_mainDbContext.Flights,
                    routeWithSubscription => routeWithSubscription.Route.Id,
                    flight => flight.RouteId,
                    (routeWithSubscription, flight) => new FlightChangeResult()
                    {
                        FlightId = flight.Id,
                        OriginCityId = routeWithSubscription.Route.OriginCityId,
                        DestinationCityId = routeWithSubscription.Route.DestinationCityId,
                        DepartureTime = flight.DepartureTime,
                        ArrivalTime = flight.ArrivalTime,
                        AirlineId = flight.AirlineId,
                    })
                .ToListAsync(ct);


            var groupedFlights = flightChangeResultList
                .GroupBy(f => new { f.AirlineId, f.OriginCityId, f.DestinationCityId })
                .ToList();

            foreach (var group in groupedFlights)
            {
                // Order flights by DepartureTime
                var flights = group.OrderBy(f => f.DepartureTime).ToList();

                for (int i = 0; i < flights.Count; i++)
                {
                    var flight = flights[i];

                    // Check for new flight
                    bool isNewFlight = true;
                    if (i > 0)
                    {
                        var previousFlight = flights[i - 1];
                        if (Math.Abs((flight.DepartureTime - previousFlight.DepartureTime.AddDays(DayCount)).TotalMinutes) <= Tolerance)
                        {
                            isNewFlight = false;
                        }
                    }

                    // Check for discontinued flight
                    bool isDiscontinuedFlight = true;
                    if (i < flights.Count - 1)
                    {
                        var nextFlight = flights[i + 1];
                        if (Math.Abs((flight.DepartureTime - nextFlight.DepartureTime.AddDays(-DayCount)).TotalMinutes) <= Tolerance)
                        {
                            isDiscontinuedFlight = false;
                        }
                    }

                    // Set the status based on the checks
                    if (isNewFlight)
                    {
                        flight.Status = "New";
                    }
                    else if (isDiscontinuedFlight)
                    {
                        flight.Status = "Discontinued";
                    }
                    else
                    {
                        flight.Status = "Unchanged";
                    }
                }
            }

            return flightChangeResultList;
        }
    }
}
