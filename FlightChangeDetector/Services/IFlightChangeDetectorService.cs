using FlightChangeDetector.Models;

namespace FlightChangeDetector.Services
{
    public interface IFlightChangeDetectorService
    {
        Task<List<FlightChangeResult>> DetectFlightChangesAsync(DateTime startDate, DateTime endDate, int agencyId, CancellationToken ct = default);
    }
}
