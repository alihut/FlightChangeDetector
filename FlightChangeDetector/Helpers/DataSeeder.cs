using FlightChangeDetector.Domain.Entities;
using FlightChangeDetector.Domain;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FlightChangeDetector.Helpers
{
    public static class DataSeeder
    {
        public static void SeedData(MainDbContext context)
        {
            // Check if data already exists
            if (context.Flights.Any()) return;

            Console.WriteLine("Data seeding started.");
            using var transaction = context.Database.BeginTransaction();

            try
            {
                // Save routes, flights, and subscriptions
                SaveEntities<Route>(context, "routes.csv", MapToRoute);
                SaveEntities<Flight>(context, "flights.csv", MapToFlight);
                SaveEntities<Subscription>(context, "subscriptions.csv", MapToSubscription);

                // Commit the transaction
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Rollback the transaction on error
                transaction.Rollback();
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Data seeding completed.");
        }

        private static void SaveEntities<TEntity>(MainDbContext context, string csvFileName, Func<CsvReader, TEntity> mapFunction)
            where TEntity : class
        {
            string currentDirectory = Environment.CurrentDirectory;
            string filePath = Path.Combine(currentDirectory, csvFileName);

            var entities = LoadEntities(filePath, mapFunction);

            if (typeof(TEntity) == typeof(Flight) || typeof(TEntity) == typeof(Route))
            {
                context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[{typeof(TEntity).Name}s] ON");
                context.Set<TEntity>().AddRange(entities);
                context.SaveChanges();
                context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [dbo].[{typeof(TEntity).Name}s] OFF");
            }
            else
            {
                context.Set<TEntity>().AddRange(entities);
                context.SaveChanges();
            }
        }

        private static List<TEntity> LoadEntities<TEntity>(string filePath, Func<CsvReader, TEntity> mapFunction)
        {
            var entities = new List<TEntity>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false,
                BadDataFound = null,
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                while (csv.Read())
                {
                    var entity = mapFunction(csv);
                    entities.Add(entity);
                }
            }

            return entities;
        }

        private static Flight MapToFlight(CsvReader csv)
        {
            return new Flight
            {
                Id = csv.GetField<int>(0),
                RouteId = csv.GetField<int>(1),
                DepartureTime = csv.GetField<DateTime>(2),
                ArrivalTime = csv.GetField<DateTime>(3),
                AirlineId = csv.GetField<int>(4)
            };
        }

        private static Route MapToRoute(CsvReader csv)
        {
            return new Route
            {
                Id = csv.GetField<int>(0),
                OriginCityId = csv.GetField<int>(1),
                DestinationCityId = csv.GetField<int>(2),
                DepartureDate = csv.GetField<DateTime>(3)
            };
        }

        private static Subscription MapToSubscription(CsvReader csv)
        {
            return new Subscription
            {
                AgencyId = csv.GetField<int>(0),
                OriginCityId = csv.GetField<int>(1),
                DestinationCityId = csv.GetField<int>(2)
            };
        }
    }


}
