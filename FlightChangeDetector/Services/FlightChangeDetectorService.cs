using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightChangeDetector.Domain;
using FlightChangeDetector.Domain.Entities;
using FlightChangeDetector.Models;
using Microsoft.Data.SqlClient;
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
        private readonly TimeSpan tolerance = TimeSpan.FromMinutes(30);

        public async Task<List<FlightChangeResult>> DetectFlightChangesAsync(DateTime startDate, DateTime endDate, int agencyId, CancellationToken ct = default)
        {


            //var query = await _mainDbContext.Subscriptions
            //    .Where(s => s.AgencyId == agencyId)
            //    .Join(_mainDbContext.Routes,
            //        subscription => new { subscription.OriginCityId, subscription.DestinationCityId },
            //        route => new { route.OriginCityId, route.DestinationCityId },
            //        (subscription, route) => new { Subscription = subscription, Route = route })
            //    .Join(_mainDbContext.Flights,
            //        routeWithSubscription => routeWithSubscription.Route.Id,
            //        flight => flight.RouteId,
            //        (routeWithSubscription, flight) => flight)
            //    .ToListAsync(ct);

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
                        if (Math.Abs((flight.DepartureTime - previousFlight.DepartureTime.AddDays(7)).TotalMinutes) <= 30)
                        {
                            isNewFlight = false;
                        }
                    }

                    // Check for discontinued flight
                    bool isDiscontinuedFlight = true;
                    if (i < flights.Count - 1)
                    {
                        var nextFlight = flights[i + 1];
                        if (Math.Abs((flight.DepartureTime - nextFlight.DepartureTime.AddDays(-7)).TotalMinutes) <= 30)
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

            //todo: ilkçil
            //foreach (var flight in flightChangeResultList)
            //{
            //    // Check for new flights
            //    var newFlight = flightChangeResultList.Any(f =>
            //        f.AirlineId == flight.AirlineId &&
            //        f.OriginCityId == flight.OriginCityId &&
            //        f.DestinationCityId == flight.DestinationCityId &&
            //        Math.Abs((f.DepartureTime - flight.DepartureTime.AddDays(-7)).TotalMinutes) <= 30);

            //    // Check for discontinued flights
            //    var discontinuedFlight = flightChangeResultList.Any(f =>
            //        f.AirlineId == flight.AirlineId &&
            //        f.OriginCityId == flight.OriginCityId &&
            //        f.DestinationCityId == flight.DestinationCityId &&
            //        Math.Abs((f.DepartureTime - flight.DepartureTime.AddDays(7)).TotalMinutes) <= 30);

            //    // Set the status based on the presence of corresponding flights
            //    if (!newFlight)
            //    {
            //        flight.Status = "New";
            //    }
            //    else if (!discontinuedFlight)
            //    {
            //        flight.Status = "Discontinued";
            //    }
            //    else
            //    {
            //        flight.Status = "Unchanged";
            //    }
            //}
            //todo: ilkçil
            //var agencySubscriptions = await _mainDbContext.Subscriptions.Where(s => s.AgencyId == agencyId).ToListAsync(ct);

            //var cityPairs = agencySubscriptions
            //    .Select(s => new { s.OriginCityId, s.DestinationCityId })
            //    .Distinct()
            //    .ToList();

            //var allRoutes = await _mainDbContext.Routes.ToListAsync(ct);

            //var agencyRoutes = allRoutes
            //    .Where(r => cityPairs
            //        .Any(cp => cp.OriginCityId == r.OriginCityId && cp.DestinationCityId == r.DestinationCityId))
            //    .ToList();

            //var agencyRoutesIds = agencyRoutes.Select(r => r.Id).ToList();

            //var flights = await _mainDbContext.Flights
            //    .Where(f => agencyRoutesIds.Contains(f.RouteId))
            //    .ToListAsync(ct);

            //flights = flights.Where(f => f.Route.DepartureDate >= startDate && f.Route.DepartureDate <= endDate).ToList();

            //var flightChanges = new List<FlightChangeResult>();

            //var newFlights = flights
            //    .Where(flight => !flights.Any(existingFlight =>
            //        existingFlight.AirlineId == flight.AirlineId &&
            //        existingFlight.DepartureTime >= flight.DepartureTime.AddDays(-7) - tolerance &&
            //        existingFlight.DepartureTime <= flight.DepartureTime.AddDays(-7) + tolerance))
            //    .Select(flight => new FlightChangeResult
            //    {
            //        FlightId = flight.Id,
            //        OriginCityId = flight.Route.OriginCityId,
            //        DestinationCityId = flight.Route.DestinationCityId,
            //        DepartureTime = flight.DepartureTime,
            //        ArrivalTime = flight.ArrivalTime,
            //        AirlineId = flight.AirlineId,
            //        Status = "New"
            //    }).ToList();
            //flightChanges.AddRange(newFlights);

            //var discFlights = flights
            //    .Where(flight => !flights.Any(existingFlight =>
            //        existingFlight.AirlineId == flight.AirlineId &&
            //        existingFlight.DepartureTime >= flight.DepartureTime.AddDays(7) - tolerance &&
            //        existingFlight.DepartureTime <= flight.DepartureTime.AddDays(7) + tolerance))
            //    .Select(flight => new FlightChangeResult
            //    {
            //        FlightId = flight.Id,
            //        OriginCityId = flight.Route.OriginCityId,
            //        DestinationCityId = flight.Route.DestinationCityId,
            //        DepartureTime = flight.DepartureTime,
            //        ArrivalTime = flight.ArrivalTime,
            //        AirlineId = flight.AirlineId,
            //        Status = "Discontinued"
            //    }).ToList();

            //flightChanges.AddRange(discFlights);

            return flightChangeResultList;
        }


        //        public async Task<List<FlightChangeResult>> DetectFlightChangesAsync(DateTime startDate, DateTime endDate, int agencyId, CancellationToken ct = default)
        //        {
        //            var sql = @"WITH SubscriptionFlights AS (
        //    SELECT 
        //        f.Id AS FlightId,
        //        r.OriginCityId,
        //        r.DestinationCityId,
        //        f.DepartureTime,
        //        f.ArrivalTime,
        //        f.AirlineId
        //    FROM Flights f
        //    INNER JOIN Routes r ON f.RouteId = r.Id
        //    INNER JOIN Subscriptions s ON r.OriginCityId = s.OriginCityId 
        //                                AND r.DestinationCityId = s.DestinationCityId
        //    WHERE f.IsDeleted = 0 
        //      AND f.IsActive = 1 
        //      AND r.IsDeleted = 0 
        //      AND r.IsActive = 1
        //      AND s.IsDeleted = 0 
        //      AND s.IsActive = 1
        //),
        //NewFlights AS (
        //    SELECT 
        //        sf.FlightId,
        //        sf.OriginCityId,
        //        sf.DestinationCityId,
        //        sf.DepartureTime,
        //        sf.ArrivalTime,
        //        sf.AirlineId,
        //        'New' AS Status
        //    FROM SubscriptionFlights sf
        //    LEFT JOIN Flights f2 ON sf.AirlineId = f2.AirlineId
        //                        AND sf.OriginCityId = (SELECT r2.OriginCityId FROM Routes r2 WHERE f2.RouteId = r2.Id)
        //                        AND sf.DestinationCityId = (SELECT r2.DestinationCityId FROM Routes r2 WHERE f2.RouteId = r2.Id)
        //                        AND f2.DepartureTime BETWEEN DATEADD(MINUTE, -30, DATEADD(DAY, -7, sf.DepartureTime)) 
        //                                                 AND DATEADD(MINUTE, 30, DATEADD(DAY, -7, sf.DepartureTime))
        //    WHERE f2.Id IS NULL
        //),
        //DiscontinuedFlights AS (
        //    SELECT 
        //        sf.FlightId,
        //        sf.OriginCityId,
        //        sf.DestinationCityId,
        //        sf.DepartureTime,
        //        sf.ArrivalTime,
        //        sf.AirlineId,
        //        'Discontinued' AS Status
        //    FROM SubscriptionFlights sf
        //    LEFT JOIN Flights f2 ON sf.AirlineId = f2.AirlineId
        //                        AND sf.OriginCityId = (SELECT r2.OriginCityId FROM Routes r2 WHERE f2.RouteId = r2.Id)
        //                        AND sf.DestinationCityId = (SELECT r2.DestinationCityId FROM Routes r2 WHERE f2.RouteId = r2.Id)
        //                        AND f2.DepartureTime BETWEEN DATEADD(MINUTE, -30, DATEADD(DAY, 7, sf.DepartureTime)) 
        //                                                 AND DATEADD(MINUTE, 30, DATEADD(DAY, 7, sf.DepartureTime))
        //    WHERE f2.Id IS NULL
        //)
        //SELECT * FROM NewFlights
        //UNION ALL
        //SELECT * FROM DiscontinuedFlights;
        //";


        //            using (var command = _mainDbContext.Database.GetDbConnection().CreateCommand())
        //            {
        //                command.CommandText = sql;
        //                command.CommandType = System.Data.CommandType.Text;

        //                command.Parameters.Add(new SqlParameter("@startDate", startDate));
        //                command.Parameters.Add(new SqlParameter("@endDate", endDate));
        //                command.Parameters.Add(new SqlParameter("@agencyId", agencyId));

        //                await _mainDbContext.Database.OpenConnectionAsync();

        //                using (var result = await command.ExecuteReaderAsync())
        //                {
        //                    var flightChanges = new List<FlightChangeResult>();

        //                    while (await result.ReadAsync())
        //                    {
        //                        flightChanges.Add(new FlightChangeResult
        //                        {
        //                            FlightId = result.GetInt32(0),
        //                            OriginCityId = result.GetInt32(1),
        //                            DestinationCityId = result.GetInt32(2),
        //                            DepartureTime = result.GetDateTime(3),
        //                            ArrivalTime = result.GetDateTime(4),
        //                            AirlineId = result.GetInt32(5),
        //                            Status = result.GetString(6)
        //                        });
        //                    }

        //                    return flightChanges;
        //                }


        //        //        var parameters = new[]
        //        //    {
        //        //    new SqlParameter("@startDate", startDate),
        //        //    new SqlParameter("@endDate", endDate),
        //        //    new SqlParameter("@agencyId", agencyId)
        //        //};
        //        //    var flightChanges = await _mainDbContext.Database.SqlQueryRaw<FlightChangeResult>(sql, parameters).ToListAsync(ct);

        //        //    return flightChanges;
        //        }

        //        //public async Task<List<FlightChangeResult>> DetectFlightChangesAsync(DateTime startDate, DateTime endDate, int agencyId, CancellationToken ct = default)
        //        //{
        //        //    // Get the subscription routes for the given agency
        //        //    var subscriptionRoutes = await _mainDbContext.Subscriptions
        //        //        .Where(s => s.AgencyId == agencyId)
        //        //        .Select(s => new { s.OriginCityId, s.DestinationCityId })
        //        //        .ToListAsync(ct);


        //        //    //var tey = SqlServerDbFunctionsExtensions.DateDiffDay(EF.Functions, DateTime.Now, DateTime.Now);
        //        //    // Get flights within the date range that match the subscription routes
        //        //    var flights = await _mainDbContext.Flights
        //        //        .Include(f => f.Route)
        //        //        .Where(f => SqlServerDbFunctionsExtensions.DateDiffDay(EF.Functions, startDate, f.Route.DepartureDate) >= 0 && 
        //        //                    SqlServerDbFunctionsExtensions.DateDiffDay(EF.Functions, f.Route.DepartureDate, endDate) >= 0)
        //        //        .Join(subscriptionRoutes,
        //        //              f => new { f.Route.OriginCityId, f.Route.DestinationCityId },
        //        //              sr => new { sr.OriginCityId, sr.DestinationCityId },
        //        //              (f, sr) => f)
        //        //        .ToListAsync(ct);

        //        //    var flightChanges = new List<FlightChangeResult>();

        //        //    foreach (var flight in flights)
        //        //    {
        //        //        // Check if a flight with similar parameters existed 7 days before
        //        //        var newFlight = await _mainDbContext.Flights
        //        //            .Include(f => f.Route)
        //        //            .Where(f => f.AirlineId == flight.AirlineId)
        //        //            .Where(f => f.Route.OriginCityId == flight.Route.OriginCityId && f.Route.DestinationCityId == flight.Route.DestinationCityId)
        //        //            .Where(f => f.DepartureTime >= flight.DepartureTime.AddDays(-7).AddMinutes(-30) &&
        //        //                        f.DepartureTime <= flight.DepartureTime.AddDays(-7).AddMinutes(30))
        //        //            .FirstOrDefaultAsync(ct);

        //        //        if (newFlight == null)
        //        //        {
        //        //            flightChanges.Add(new FlightChangeResult
        //        //            {
        //        //                FlightId = flight.Id,
        //        //                OriginCityId = flight.Route.OriginCityId,
        //        //                DestinationCityId = flight.Route.DestinationCityId,
        //        //                DepartureTime = flight.DepartureTime,
        //        //                ArrivalTime = flight.ArrivalTime,
        //        //                AirlineId = flight.AirlineId,
        //        //                Status = "New"
        //        //            });
        //        //        }

        //        //        // Check if a flight with similar parameters existed 7 days after
        //        //        var discontinuedFlight = await _mainDbContext.Flights
        //        //            .Include(f => f.Route)
        //        //            .Where(f => f.AirlineId == flight.AirlineId)
        //        //            .Where(f => f.Route.OriginCityId == flight.Route.OriginCityId && f.Route.DestinationCityId == flight.Route.DestinationCityId)
        //        //            .Where(f => f.DepartureTime >= flight.DepartureTime.AddDays(7).AddMinutes(-30) &&
        //        //                        f.DepartureTime <= flight.DepartureTime.AddDays(7).AddMinutes(30))
        //        //            .FirstOrDefaultAsync(ct);

        //        //        if (discontinuedFlight == null)
        //        //        {
        //        //            flightChanges.Add(new FlightChangeResult
        //        //            {
        //        //                FlightId = flight.Id,
        //        //                OriginCityId = flight.Route.OriginCityId,
        //        //                DestinationCityId = flight.Route.DestinationCityId,
        //        //                DepartureTime = flight.DepartureTime,
        //        //                ArrivalTime = flight.ArrivalTime,
        //        //                AirlineId = flight.AirlineId,
        //        //                Status = "Discontinued"
        //        //            });
        //        //        }
        //        //    }

        //        //    return flightChanges;
        //        //}


        //        //public async Task<List<FlightChangeResult>> DetectFlightChangesAsync(DateTime startDate, DateTime endDate, int agencyId, CancellationToken ct = default)
        //        //{
        //        //    var subscriptionRoutes = await _mainDbContext.Subscriptions
        //        //        .Where(s => s.AgencyId == agencyId)
        //        //        .Select(s => new { s.OriginCityId, s.DestinationCityId })
        //        //        .ToListAsync(ct);

        //        //    var flights = await _mainDbContext.Flights
        //        //        .Include(f => f.Route)
        //        //        .Where(f => f.Route.DepartureDate >= startDate && f.Route.DepartureDate <= endDate)
        //        //        .Where(f => subscriptionRoutes.Any(sr =>
        //        //            sr.OriginCityId == f.Route.OriginCityId && sr.DestinationCityId == f.Route.DestinationCityId))
        //        //        .ToListAsync(ct);

        //        //    var flightChanges = new List<FlightChangeResult>();

        //        //    foreach (var flight in flights)
        //        //    {
        //        //        var newFlight = await _mainDbContext.Flights
        //        //            .Include(f => f.Route)
        //        //            .Where(f => f.AirlineId == flight.AirlineId)
        //        //            .Where(f => f.Route.OriginCityId == flight.Route.OriginCityId && f.Route.DestinationCityId == flight.Route.DestinationCityId)
        //        //            .Where(f => f.DepartureTime >= flight.DepartureTime.AddDays(-7).AddMinutes(-30) &&
        //        //                        f.DepartureTime <= flight.DepartureTime.AddDays(-7).AddMinutes(30))
        //        //            .FirstOrDefaultAsync(ct);

        //        //        if (newFlight == null)
        //        //        {
        //        //            flightChanges.Add(new FlightChangeResult
        //        //            {
        //        //                FlightId = flight.Id,
        //        //                OriginCityId = flight.Route.OriginCityId,
        //        //                DestinationCityId = flight.Route.DestinationCityId,
        //        //                DepartureTime = flight.DepartureTime,
        //        //                ArrivalTime = flight.ArrivalTime,
        //        //                AirlineId = flight.AirlineId,
        //        //                Status = "New"
        //        //            });
        //        //        }

        //        //        var discontinuedFlight = await _mainDbContext.Flights
        //        //            .Include(f => f.Route)
        //        //            .Where(f => f.AirlineId == flight.AirlineId)
        //        //            .Where(f => f.Route.OriginCityId == flight.Route.OriginCityId && f.Route.DestinationCityId == flight.Route.DestinationCityId)
        //        //            .Where(f => f.DepartureTime >= flight.DepartureTime.AddDays(7).AddMinutes(-30) &&
        //        //                        f.DepartureTime <= flight.DepartureTime.AddDays(7).AddMinutes(30))
        //        //            .FirstOrDefaultAsync(ct);

        //        //        if (discontinuedFlight == null)
        //        //        {
        //        //            flightChanges.Add(new FlightChangeResult
        //        //            {
        //        //                FlightId = flight.Id,
        //        //                OriginCityId = flight.Route.OriginCityId,
        //        //                DestinationCityId = flight.Route.DestinationCityId,
        //        //                DepartureTime = flight.DepartureTime,
        //        //                ArrivalTime = flight.ArrivalTime,
        //        //                AirlineId = flight.AirlineId,
        //        //                Status = "Discontinued"
        //        //            });
        //        //        }
        //        //    }

        //        //    return flightChanges;
        //        //}
        //    }
    }
}
