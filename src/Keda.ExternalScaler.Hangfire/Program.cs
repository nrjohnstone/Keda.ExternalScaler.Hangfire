using System;
using Common.Utilities.Configuration;
using Keda.ExternalScaler.Hangfire.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Keda.ExternalScaler.Hangfire
{
    class Program
    {
        public const string ApplicationName = "Keda.ExternalScaler.Hangfire";

        static void Main(string[] args)
        {
            var applicationConfiguration = new ApplicationConfiguration(args, ApplicationName).CreateBuilder().Build();

            ISettings settings = applicationConfiguration.ToSettings<Settings>(ApplicationName);

            Log.Logger = SerilogConfiguration.Create(ApplicationName, settings.MinimumLogLevel,
                settings.HttpLogEndpoint);

            if (settings.HangfireSqlInstances == null)
            {
                Log.Error("There are no hangfire instances defined in configuration");
                Environment.Exit(-1);
            }

            Log.Information("Starting {ApplicationName} v1.0", ApplicationName);

            var applicationHostBuilder = new ApplicationHostBuilder(args, settings);

            var hostBuilder = applicationHostBuilder.Create();
            hostBuilder.Build().Run();
        }
    }
}
