using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.SystemConsole.Themes;

namespace Tests.Unit
{
    internal class SerilogConfiguration
    {
        public static ILogger Create(string applicationName, LogEventLevel minimumLevel)
        {
            var configuration = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.WithProcessName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.FromLogContext()
                .MinimumLevel.Is(minimumLevel)
                .WriteTo.InMemory()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message,-30:lj} {Properties:j}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Literate);

            return configuration.CreateLogger();
        }
    }
}