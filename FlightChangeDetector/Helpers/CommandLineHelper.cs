using System.Globalization;

namespace FlightChangeDetector.Helpers
{
    public static class CommandLineHelper
    {
        public static bool TryParseArguments(string[] args, out DateTime startDate, out DateTime endDate, out int agencyId)
        {
            startDate = default;
            endDate = default;
            agencyId = default;

            if (args.Length != 3)
            {
                Console.WriteLine("Usage: FlightScheduleApp <start date> <end date> <agency id>");
                return false;
            }

            if (!DateTime.TryParseExact(args[0], "yyyy-MM-dd", null, DateTimeStyles.None, out startDate))
            {
                Console.WriteLine("Invalid start date format. Please use yyyy-MM-dd.");
                return false;
            }

            if (!DateTime.TryParseExact(args[1], "yyyy-MM-dd", null, DateTimeStyles.None, out endDate))
            {
                Console.WriteLine("Invalid end date format. Please use yyyy-MM-dd.");
                return false;
            }

            if (!int.TryParse(args[2], out agencyId))
            {
                Console.WriteLine("Invalid agency ID. Please provide a valid integer value.");
                return false;
            }

            return true;
        }
    }

}
