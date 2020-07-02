using System.Collections.Generic;
using Hangfire.Storage;

namespace Keda.ExternalScaler.Hangfire
{
    /// <summary>
    /// Provide keyed access to a collection of Hangfire MonitoringApi instances
    /// </summary>
    public class HangfireMonitoringRouter : IHangfireMetricsApi
    {
        private readonly Dictionary<string, IMonitoringApi> _monitoringApis;

        public HangfireMonitoringRouter()
        {
            _monitoringApis = new Dictionary<string, IMonitoringApi>();
        }

        public void AddMonitoringApi(string hangfireInstanceName, IMonitoringApi monitoringApi)
        {
            _monitoringApis.Add(hangfireInstanceName, monitoringApi);
        }

        public long EnqueuedCount(string hangfireInstanceName, string queueName)
        {
            return _monitoringApis[hangfireInstanceName].EnqueuedCount(queueName);
        }

        public long FetchedCount(string hangfireInstanceName, string queueName)
        {
            return _monitoringApis[hangfireInstanceName].FetchedCount(queueName);
        }
    }
}