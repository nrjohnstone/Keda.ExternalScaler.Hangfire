using System.Collections.Generic;
using Serilog.Events;

namespace Hangfire.Producer.Configuration
{
    public class Settings : ISettings
    {
        public string HttpLogEndpoint { get; set; }
        public LogEventLevel MinimumLogLevel { get; set; } = LogEventLevel.Debug;
        public IEnumerable<HangfireSqlServerSettings> HangfireInstances { get; set; }
        public string RequestType { get; set; } = "addition";
        public int NumberOfJobs { get; set; } = 1;
    }
}