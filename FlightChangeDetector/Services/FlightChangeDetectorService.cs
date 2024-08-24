using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightChangeDetector.Domain;

namespace FlightChangeDetector.Services
{
    public class FlightChangeDetectorService
    {
        private readonly MainDbContext _mainDbContext;

        public FlightChangeDetectorService(MainDbContext mainDbContext)
        {
            _mainDbContext = mainDbContext;
        }


    }
}
