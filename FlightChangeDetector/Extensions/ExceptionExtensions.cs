using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlightChangeDetector.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetCompleteMessage(this Exception ex)
        {
            var messageBuilder = new StringBuilder();

            void CollectMessages(Exception exception)
            {
                messageBuilder.AppendLine(exception.Message);

                // Recursively collect messages from inner exceptions
                if (exception.InnerException != null)
                {
                    CollectMessages(exception.InnerException);
                }
            }

            CollectMessages(ex);

            return messageBuilder.ToString();
        }

    }
}
