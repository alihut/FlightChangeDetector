using FlightChangeDetector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightChangeDetector.Services
{
    public interface IFlightChangeDetectorService
    {
        Task<List<FlightChangeResult>> DetectFlightChangesAsync(DateTime startDate, DateTime endDate, int agencyId, CancellationToken ct = default);
    }
}
