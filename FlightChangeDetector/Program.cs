using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FlightChangeDetector.Domain;
using FlightChangeDetector.Helpers;
using FlightChangeDetector.Models;
using FlightChangeDetector.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Starting the application...");

var cs = "Data Source=localhost;Initial Catalog=FlightDb;User ID=sa;Password=123456;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Pooling=true;Min Pool Size=5;Max Pool Size=100;";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register the DbContext with a connection string
        services.AddDbContext<MainDbContext>(options =>
            options.UseSqlServer(cs));

        services.AddScoped<IFlightChangeDetectorService, FlightChangeDetectorService>();

        // Register other services here if needed
    })
    .Build();

if (args.Length != 3)
{
    Console.WriteLine("Usage: FlightScheduleApp <start date> <end date> <agency id>");
    return;
}

// Parse the start date
if (!DateTime.TryParseExact(args[0], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
{
    Console.WriteLine("Invalid start date format. Please use yyyy-MM-dd.");
    return;
}

// Parse the end date
if (!DateTime.TryParseExact(args[1], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
{
    Console.WriteLine("Invalid end date format. Please use yyyy-MM-dd.");
    return;
}

// Parse the agency ID
if (!int.TryParse(args[2], out int agencyId))
{
    Console.WriteLine("Invalid agency ID. Please provide a valid integer value.");
    return;
}

// Proceed with your business logic here
Console.WriteLine($"Processing flight schedule from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} for agency ID: {agencyId}");

var flightChangeDetectorService = host.Services.GetRequiredService<IFlightChangeDetectorService>();

Stopwatch sw = Stopwatch.StartNew();
List<FlightChangeResult> results = await flightChangeDetectorService.DetectFlightChangesAsync(startDate, endDate, agencyId);
sw.Stop();
var detectFlightChangesTime = sw.ElapsedMilliseconds;
sw = Stopwatch.StartNew();
WriteResultsToCsv(results, "results.csv");
sw.Stop();
var writeToCsvtime = sw.ElapsedMilliseconds;

Console.WriteLine($"Processing finished. detectFlightChangesTime : {detectFlightChangesTime}, writeToCsvtime : {writeToCsvtime}");

//using (var scope = host.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var context = services.GetRequiredService<MainDbContext>();

//        // Seed data from CSV files
//        //DataSeeder.SeedData(context);
//        DataSeeder.SeedData(context);

//        Console.WriteLine("Data seeding completed.");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"An error occurred: {ex.Message}");
//    }
//}

Console.WriteLine("Application is running...");

// Optionally, you can keep the host running if your application needs to stay active.
// If this is a long-running process, uncomment the line below.
host.Run();

static void WriteResultsToCsv(IEnumerable<FlightChangeResult> results, string filePath)
{
    using (var writer = new StreamWriter(filePath))
    using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
           {
               Delimiter = ","
           }))
    {
        // Write the header
        csv.WriteHeader<FlightChangeResult>();
        csv.NextRecord();

        // Write the records
        csv.WriteRecords(results);
    }

    Console.WriteLine($"Data successfully written to {filePath}");
}