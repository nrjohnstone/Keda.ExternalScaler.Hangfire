using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Common.Utilities.Configuration
{
    public class SerilogConfiguration
    {
        public static ILogger Create(string applicationName, LogEventLevel minimumLevel, string requestUri)
        {
            var configuration = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.WithProcessName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithProperty("BuildVersion", Globals.BuildVersion)
                .Enrich.FromLogContext()
                .MinimumLevel.Is(minimumLevel)
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message,-30:lj} {Properties:j}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Literate);

            // Optional logging to an HTTP endpoint such as Logstash
            if (!string.IsNullOrEmpty(requestUri))
            {
                configuration.WriteTo.Http(requestUri);
            }

            return configuration.CreateLogger();
        }
    }
}