using System.Collections.Generic;
using Serilog.Events;

namespace Hangfire.Consumer.Configuration
{
    public class Settings : ISettings
    {
        public string HttpLogEndpoint { get; set; }
        public LogEventLevel MinimumLogLevel { get; set; }
        public HangfireSqlServerSettings Hangfire { get; set; }
        public string HangfireQueue { get; set; }
        public int IdleTimeoutSeconds { get; set; }
    }
}