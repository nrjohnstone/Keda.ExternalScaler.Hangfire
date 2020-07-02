using System.Collections;
using System.Collections.Generic;
using Hangfire.Storage;
using Keda.ExternalScaler.Hangfire;
using Keda.ExternalScaler.Hangfire.Configuration;

namespace Tests.Unit
{
    /// <summary>
    /// Test Double of the ApplicationHostBuilder that allows us to replace implementations
    /// for in memory unit testing
    /// </summary>
    internal class ApplicationHostBuilderTestDouble : ApplicationHostBuilder
    {
        private readonly IHangfireScaledObjectRepository _scaledObjectRepository;
        private readonly IList<HangfireMonitorTestDouble> _monitoringApis;

        public ApplicationHostBuilderTestDouble(string[] args, ISettings settings, 
            IHangfireScaledObjectRepository scaledObjectRepository,
            IList<HangfireMonitorTestDouble> monitoringApis) : base(args, settings)
        {
            _scaledObjectRepository = scaledObjectRepository;
            _monitoringApis = monitoringApis;
        }

        protected override IHangfireScaledObjectRepository CreateHangfireScaledObjectRepository()
        {
            return _scaledObjectRepository;
        }

        protected override IMonitoringApi CreateHangfireMonitor(HangfireSqlServerSettings sqlServerSettings)
        {
            var hangfireMonitorTestDouble = new HangfireMonitorTestDouble(sqlServerSettings.Name);
            _monitoringApis.Add(hangfireMonitorTestDouble);
            return hangfireMonitorTestDouble;
        }

    }
}