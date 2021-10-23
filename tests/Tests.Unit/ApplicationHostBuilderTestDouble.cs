using System.Collections.Generic;
using Hangfire.Storage;
using HangfireExternalScaler.Configuration;

namespace HangfireExternalScaler.Tests.Unit
{
    /// <summary>
    /// Test Double of the ApplicationHostBuilder that allows us to replace implementations
    /// for in memory unit testing
    /// </summary>
    internal class ApplicationHostBuilderTestDouble : ApplicationHostBuilder
    {
        private readonly IList<HangfireMonitorTestDouble> _monitoringApis;

        public ApplicationHostBuilderTestDouble(string[] args, ISettings settings, 
            IList<HangfireMonitorTestDouble> monitoringApis) : base(args, settings)
        {
            _monitoringApis = monitoringApis;
        }

        protected override IMonitoringApi CreateHangfireMonitor(HangfireSqlServerSettings sqlServerSettings)
        {
            var hangfireMonitorTestDouble = new HangfireMonitorTestDouble(sqlServerSettings.Name);
            _monitoringApis.Add(hangfireMonitorTestDouble);
            return hangfireMonitorTestDouble;
        }

    }
}