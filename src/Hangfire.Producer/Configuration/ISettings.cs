using System.Collections.Generic;
using Serilog.Events;

namespace Hangfire.Producer.Configuration
{
    public interface ISettings
    {
        string HttpLogEndpoint { get; set; } 
        LogEventLevel MinimumLogLevel { get; set; }
        IEnumerable<HangfireSqlServerSettings> HangfireInstances { get; set; }
        string RequestType { get; }
        int NumberOfJobs { get; }
    }
}