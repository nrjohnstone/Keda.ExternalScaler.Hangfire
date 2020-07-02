using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Utilities.Configuration;
using Hangfire.Dashboard.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Hangfire.Dashboard
{
    public class Program
    {
        public const string ApplicationName = "Hangfire.Dashboard";

        public static void Main(string[] args)
        {
            var applicationConfiguration = new ApplicationConfiguration(args, ApplicationName).CreateBuilder().Build();

            ISettings settings = applicationConfiguration.ToSettings<Settings>(ApplicationName);

            Log.Logger = SerilogConfiguration.Create(ApplicationName, settings.MinimumLogLevel,
                settings.HttpLogEndpoint);

            Log.Information("Starting {ApplicationName} v1.0", ApplicationName);

            var applicationHostBuilder = new ApplicationHostBuilder(args, ApplicationName, applicationConfiguration, settings);

            var hostBuilder = applicationHostBuilder.Create();

            hostBuilder.Build().Run();
        }
    }
}
