using System.Collections.Generic;
using Serilog.Events;

namespace Hangfire.Consumer.Configuration
{
    public interface ISettings
    {
        string HttpLogEndpoint { get; set; } 
        LogEventLevel MinimumLogLevel { get; set; }
        HangfireSqlServerSettings Hangfire{ get; set; }
        string HangfireQueue { get; set; }
        int IdleTimeoutSeconds { get; set; }
    }
}