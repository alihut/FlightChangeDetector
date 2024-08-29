using System.Diagnostics;

namespace FlightChangeDetector.Helpers
{
    public static class TimingHelper
    {
        public static void ExecuteWithTiming(Action action)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            Console.WriteLine($"Processing finished in {sw.ElapsedMilliseconds} ms");
        }

        public static async Task ExecuteWithTimingAsync(Func<Task> action)
        {
            var sw = Stopwatch.StartNew();
            await action();
            sw.Stop();
            Console.WriteLine($"Processing finished in {sw.ElapsedMilliseconds} ms");
        }
    }

}
