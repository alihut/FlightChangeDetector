using FlightChangeDetector.Domain.Entities;
using FlightChangeDetector.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace FlightChangeDetector.Helpers
{
    public static class DataSeederNew
    {
        public static void SeedData(MainDbContext context)
        {
            // Check if data already exists
            if (context.Flights.Any()) return;

            // Load data from CSV files

            var cs = "Data Source=localhost;Initial Catalog=FlightDb;User ID=sa;Password=123456;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Pooling=true;Min Pool Size=5;Max Pool Size=100;";
            // 

            using (var connection = new SqlConnection(cs))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        SaveRoutes(connection, transaction);
                        SaveFlights(connection, transaction);
                        SaveSubscriptions(connection, transaction);

                        // Commit the transaction once all operations are successful
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction on error
                        transaction.Rollback();
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        private static void SaveFlights(SqlConnection connection, SqlTransaction transaction)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string flightsCsv = "flights.csv";
            var flights = LoadFlights(Path.Combine(currentDirectory, flightsCsv));

            // Convert flights to DataTable
            DataTable flightsTable = ConvertToDataTable(flights);

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, transaction))
            {
                bulkCopy.DestinationTableName = "Flights";
                bulkCopy.BulkCopyTimeout = 300;
                bulkCopy.WriteToServer(flightsTable);
            }
        }

        private static void SaveSubscriptions(SqlConnection connection, SqlTransaction transaction)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string subscriptionsCsv = "subscriptions.csv";
            var subscriptions = LoadSubscriptions(Path.Combine(currentDirectory, subscriptionsCsv));

            // Convert subscriptions to DataTable
            DataTable subscriptionsTable = ConvertToDataTable(subscriptions);

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.DestinationTableName = "Subscriptions";
                bulkCopy.BulkCopyTimeout = 300;
                bulkCopy.WriteToServer(subscriptionsTable);
            }
        }

        private static void SaveRoutes(SqlConnection connection, SqlTransaction transaction)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string routesCsv = "routes.csv";
            var routes = LoadRoutes(Path.Combine(currentDirectory, routesCsv));

            // Convert routes to DataTable
            DataTable routesTable = ConvertToDataTable(routes);

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, transaction))
            {
                bulkCopy.DestinationTableName = "Routes";
                bulkCopy.BulkCopyTimeout = 300;
                bulkCopy.WriteToServer(routesTable);
            }
        }

        private static DataTable ConvertToDataTable(List<Subscription> subscriptions)
        {
            var table = new DataTable();
            table.Columns.Add("AgencyId", typeof(int));
            table.Columns.Add("OriginCityId", typeof(int));
            table.Columns.Add("DestinationCityId", typeof(int));
            table.Columns.Add("IsActive", typeof(bool));
            table.Columns.Add("IsDeleted", typeof(bool));

            foreach (var subscription in subscriptions)
            {
                table.Rows.Add(subscription.AgencyId, subscription.OriginCityId, subscription.DestinationCityId, true, false);
            }


            return table;
        }

        private static DataTable ConvertToDataTable(List<Flight> flights)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("RouteId", typeof(int));
            table.Columns.Add("DepartureTime", typeof(DateTime));
            table.Columns.Add("ArrivalTime", typeof(DateTime));
            table.Columns.Add("AirlineId", typeof(int));
            table.Columns.Add("IsActive", typeof(bool));
            table.Columns.Add("IsDeleted", typeof(bool));

            foreach (var flight in flights)
            {
                table.Rows.Add(flight.Id, flight.RouteId, flight.DepartureTime, flight.ArrivalTime, flight.AirlineId, true, false);
            }

            return table;
        }

        private static DataTable ConvertToDataTable(List<Route> routes)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("OriginCityId", typeof(int));
            table.Columns.Add("DestinationCityId", typeof(int));
            table.Columns.Add("DepartureDate", typeof(DateTime));
            table.Columns.Add("IsActive", typeof(bool));
            table.Columns.Add("IsDeleted", typeof(bool));

            foreach (var route in routes)
            {
                table.Rows.Add(route.Id, route.OriginCityId, route.DestinationCityId, route.DepartureDate, true, false);
            }

            return table;
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
