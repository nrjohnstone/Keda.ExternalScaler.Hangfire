using System.Collections.Generic;
using Serilog.Events;

namespace Keda.ExternalScaler.Hangfire.Configuration
{
    public interface ISettings
    {
        string HttpLogEndpoint { get; set; } 
        LogEventLevel MinimumLogLevel { get; set; }
        IEnumerable<HangfireSqlServerSettings> HangfireSqlInstances { get; set; }
    }
}