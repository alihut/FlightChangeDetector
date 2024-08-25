using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace FlightChangeDetector.Helpers
{
    public static class CsvUtil
    {
        public static void WriteResultsToCsv<T>(IEnumerable<T> results, string filePath) where T : class
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                   {
                       Delimiter = ","
                   }))
            {
                // Write the header
                csv.WriteHeader<T>();
                csv.NextRecord();

                // Write the records
                csv.WriteRecords(results);
            }

            Console.WriteLine($"Data successfully written to {filePath}");
        }
    }
}
