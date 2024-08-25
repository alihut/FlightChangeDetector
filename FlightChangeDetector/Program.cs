using FlightChangeDetector.Domain;
using FlightChangeDetector.Extensions;
using FlightChangeDetector.Helpers;
using FlightChangeDetector.Models;
using FlightChangeDetector.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Starting the application...");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string connectionString = configuration.GetConnectionString("DefaultConnection");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<MainDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IFlightChangeDetectorService, FlightChangeDetectorService>();
    })
    .Build();

if (!CommandLineHelper.TryParseArguments(args, out DateTime startDate, out DateTime endDate, out int agencyId))
{
    return;
}

EnsureSeedData(host);


Console.WriteLine($"Processing flight schedule from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} for agency ID: {agencyId}");

var flightChangeDetectorService = host.Services.GetRequiredService<IFlightChangeDetectorService>();

TimingHelper.ExecuteWithTiming(() =>
{
    try
    {
        List<FlightChangeResult> results = flightChangeDetectorService.DetectFlightChangesAsync(startDate, endDate, agencyId).Result;
        CsvUtil.WriteResultsToCsv(results, "results.csv");
        Console.WriteLine($"Processing finished. {results.Count} flights found");
    }
    catch (Exception e)
    {
        Console.WriteLine(e.GetCompleteMessage());
    }
});


static void EnsureSeedData(IHost host)
{
    using (var scope = host.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<MainDbContext>();

            DataSeeder.SeedData(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}