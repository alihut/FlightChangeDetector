using FlightChangeDetector.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightChangeDetector.Domain;
using System.Formats.Asn1;
using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace FlightChangeDetector.Helpers
{
    public static class DataSeeder
    {
        public static void SeedData(MainDbContext context)
        {
            // Check if data already exists
            if (context.Flights.Any()) return;

            // Load data from CSV files

            using var transaction = context.Database.BeginTransaction();


            try
            {
                // Load and save routes first to ensure foreign keys are set for flights
                SaveRoutes(context);

                // Load and save flights
                SaveFlights(context);

                // Load and save subscriptions
                SaveSubscriptions(context);

                // Commit the transaction
                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Rollback the transaction on error
                transaction.Rollback();
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void SaveFlights(MainDbContext context)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string flightsCsv = "flights.csv";
            var flights = LoadFlights(Path.Combine(currentDirectory, flightsCsv));

            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Flights] ON");
            var bulkConfig = new BulkConfig { SetOutputIdentity = false,  };
            bulkConfig.PropertiesToExclude = new List<string> { "Route" };
            context.Flights.AddRange(flights);
            context.SaveChanges();
            //context.BulkSaveChanges();
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Flights] OFF");
        }

        private static void SaveRoutes(MainDbContext context)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string routesCsv = "routes.csv";
            var routes = LoadRoutes(Path.Combine(currentDirectory, routesCsv));

            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Routes] ON");
            var bulkConfig = new BulkConfig { SetOutputIdentity = false };

            //bulkConfig.PropertiesToExclude = new List<string> { "Flights" };
            context.Routes.AddRange(routes);
            context.SaveChanges();
            context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Routes] OFF");
        }

        private static void SaveSubscriptions(MainDbContext context)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string subscriptionsCsv = "subscriptions.csv";
            var subscriptions = LoadSubscriptions(Path.Combine(currentDirectory, subscriptionsCsv));


            //context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Subscriptions] ON");
            context.Subscriptions.AddRange(subscriptions);
            context.SaveChanges();
            //context.BulkSaveChanges();
            //context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT [dbo].[Subscriptions] OFF");
        }

        private static List<Flight> LoadFlights(string filePath)
        {
            var flights = new List<Flight>();

            // Set up the CSV reader configuration
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false,  // Assuming there's no header row
                BadDataFound = null,      // Ignore bad data
            };


            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                // Read each row and map it to the Flight entity


                while (csv.Read())
                {
                    var flight = new Flight
                    {
                        Id = csv.GetField<int>(0),
                        RouteId = csv.GetField<int>(1),
                        DepartureTime = csv.GetField<DateTime>(2),
                        ArrivalTime = csv.GetField<DateTime>(3),
                        AirlineId = csv.GetField<int>(4)
                    };

                    flights.Add(flight);
                }
            }

            return flights;
        }

        private static List<Route> LoadRoutes(string filePath)
        {
            var routes = new List<Route>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false,  // Assuming there's no header row
                BadDataFound = null,      // Ignore bad data
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                // Read each row and map it to the Route entity
                while (csv.Read())
                {
                    var route = new Route
                    {
                        Id = csv.GetField<int>(0),
                        OriginCityId = csv.GetField<int>(1),
                        DestinationCityId = csv.GetField<int>(2),
                        DepartureDate = csv.GetField<DateTime>(3)
                    };

                    routes.Add(route);
                }
            }

            return routes;
        }

        private static List<Subscription> LoadSubscriptions(string filePath)
        {
            var subscriptions = new List<Subscription>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false,  // Assuming there's no header row
                BadDataFound = null,      // Ignore bad data
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                // Read each row and map it to the Subscription entity
                while (csv.Read())
                {
                    var subscription = new Subscription
                    {
                        AgencyId = csv.GetField<int>(0),
                        OriginCityId = csv.GetField<int>(1),
                        DestinationCityId = csv.GetField<int>(2)
                    };

                    subscriptions.Add(subscription);
                }
            }

            return subscriptions;
        }
    }

}
